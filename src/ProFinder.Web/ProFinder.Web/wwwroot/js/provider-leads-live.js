(function () {
    const resultsContainer = document.getElementById("provider-leads-results");
    if (!resultsContainer) {
        return;
    }

    const liveStatusElement = document.getElementById("provider-leads-live-status");
    const connectionStatusElement = document.getElementById("provider-leads-connection-status");
    const resultsUrl = resultsContainer.dataset.resultsUrl;
    const hubUrl = resultsContainer.dataset.hubUrl;

    let refreshTimer = null;
    let refreshInFlight = false;
    let pendingNotification = null;

    setConnectionStatus("Conectando...", "text-muted");

    if (!window.signalR) {
        setConnectionStatus("SignalR indisponivel", "text-danger");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    connection.on("ProviderLeadsChanged", function (notification) {
        pendingNotification = notification || null;
        queueRefresh();
    });

    connection.onreconnecting(function () {
        setConnectionStatus("Reconectando...", "text-warning");
    });

    connection.onreconnected(function () {
        setConnectionStatus("Conectado", "text-success");
        queueRefresh();
    });

    connection.onclose(function () {
        setConnectionStatus("Desconectado", "text-danger");
    });

    startConnection();

    async function startConnection() {
        try {
            await connection.start();
            setConnectionStatus("Conectado", "text-success");
        } catch (error) {
            console.error("Erro ao conectar no hub de crawler runs.", error);
            setConnectionStatus("Falha na conexao", "text-danger");
            window.setTimeout(startConnection, 5000);
        }
    }

    function queueRefresh() {
        if (refreshTimer) {
            return;
        }

        refreshTimer = window.setTimeout(async function () {
            refreshTimer = null;
            await refreshResults();
        }, 900);
    }

    async function refreshResults() {
        if (refreshInFlight) {
            queueRefresh();
            return;
        }

        refreshInFlight = true;
        setLiveStatus(buildRefreshMessage(pendingNotification));

        try {
            const url = new URL(resultsUrl, window.location.origin);
            const currentQuery = new URLSearchParams(window.location.search);
            currentQuery.forEach(function (value, key) {
                url.searchParams.set(key, value);
            });

            const response = await fetch(url.toString(), {
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                throw new Error(`Falha ao atualizar resultados (${response.status}).`);
            }

            const html = await response.text();
            resultsContainer.innerHTML = html;
            setLiveStatus(buildSuccessMessage(pendingNotification));
        } catch (error) {
            console.error("Erro ao atualizar a listagem de provider leads.", error);
            setLiveStatus("Falha ao atualizar automaticamente. A pagina continuara tentando nas proximas notificacoes.");
        } finally {
            refreshInFlight = false;
        }
    }

    function setConnectionStatus(text, cssClass) {
        if (!connectionStatusElement) {
            return;
        }

        connectionStatusElement.textContent = text;
        connectionStatusElement.className = `small fw-semibold ${cssClass || ""}`.trim();
    }

    function setLiveStatus(text) {
        if (!liveStatusElement) {
            return;
        }

        liveStatusElement.textContent = text || "";
    }

    function buildRefreshMessage(notification) {
        if (!notification) {
            return "Atualizando resultados...";
        }

        if (notification.operation === "reset") {
            return "Reset do crawler concluido. Atualizando a lista...";
        }

        const lote = notification.runId ? `Lote #${notification.runId}` : "Crawler";
        return `${lote} atualizou leads. Sincronizando a lista...`;
    }

    function buildSuccessMessage(notification) {
        const updatedAt = new Date().toLocaleTimeString("pt-BR", {
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        });

        if (!notification) {
            return `Lista atualizada as ${updatedAt}.`;
        }

        if (notification.operation === "reset") {
            return `Dados do crawler resetados as ${updatedAt}.`;
        }

        const lote = notification.runId ? `Lote #${notification.runId}` : "Crawler";
        const parts = [
            `${lote} sincronizado as ${updatedAt}.`,
            `Capturados: ${notification.captured ?? 0}`,
            `Inseridos: ${notification.inserted ?? 0}`,
            `Atualizados: ${notification.updated ?? 0}`
        ];

        if (notification.status) {
            parts.push(`Status: ${notification.status}`);
        }

        return parts.join(" ");
    }
})();
