(function () {
    const container = document.getElementById("crawler-run-live");
    if (!container) {
        return;
    }

    const runId = Number(container.dataset.runId);
    const hubUrl = container.dataset.hubUrl;
    const connectionStatusElement = document.getElementById("crawler-run-connection-status");
    const liveStatusElement = document.getElementById("crawler-run-live-status");
    const stopButton = document.getElementById("crawler-run-stop-button");
    const logStream = document.getElementById("crawler-run-log-stream");
    const logEmpty = document.getElementById("crawler-run-log-empty");

    if (!window.signalR || !hubUrl || !runId) {
        setConnectionStatus("SignalR indisponivel", "text-danger");
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    connection.on("CrawlerRunUpdated", function (run) {
        if (!run || run.id !== runId) {
            return;
        }

        applyRunUpdate(run);
    });

    connection.on("CrawlerRunLogAdded", function (log) {
        if (!log || log.leadCaptureRunId !== runId) {
            return;
        }

        appendLog(log);
    });

    connection.onreconnecting(function () {
        setConnectionStatus("Reconectando...", "text-warning");
    });

    connection.onreconnected(async function () {
        setConnectionStatus("Conectado", "text-success");
        setLiveStatus("Console sincronizado em tempo real.");
        await joinGroup();
    });

    connection.onclose(function () {
        setConnectionStatus("Desconectado", "text-danger");
    });

    startConnection();

    async function startConnection() {
        try {
            await connection.start();
            await joinGroup();
            setConnectionStatus("Conectado", "text-success");
            setLiveStatus("Aguardando novos logs...");
        } catch (error) {
            console.error("Erro ao conectar no hub do crawler.", error);
            setConnectionStatus("Falha na conexao", "text-danger");
            window.setTimeout(startConnection, 5000);
        }
    }

    async function joinGroup() {
        try {
            await connection.invoke("JoinRunGroup", runId);
        } catch (error) {
            console.error("Erro ao entrar no grupo do lote.", error);
        }
    }

    function applyRunUpdate(run) {
        setText("crawler-run-status", run.status || "-");
        setText("crawler-run-items-captured", run.itemsCaptured ?? 0);
        setText("crawler-run-provider-count", run.providerLeadCount ?? 0);
        setText("crawler-run-started-at", formatDate(run.startedAt));
        setText("crawler-run-completed-at", run.completedAt ? formatDate(run.completedAt) : "-");
        setText("crawler-run-items-inserted", run.itemsInserted ?? 0);
        setText("crawler-run-items-updated", run.itemsUpdated ?? 0);
        setText("crawler-run-items-skipped", run.itemsSkipped ?? 0);
        setText("crawler-run-notes", run.notes || "");
        setVisibility("crawler-run-notes", !!run.notes);
        setVisibility("crawler-run-notes-empty", !run.notes);
        setText("crawler-run-error", run.errorMessage || "");
        setVisibility("crawler-run-error", !!run.errorMessage);
        setVisibility("crawler-run-error-empty", !run.errorMessage);
        updateStopButton(run);

        const updatedAt = new Date().toLocaleTimeString("pt-BR", {
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit"
        });
        setLiveStatus(`Lote atualizado as ${updatedAt}. Status atual: ${run.status || "-"}.`);
    }

    function appendLog(log) {
        if (logEmpty) {
            logEmpty.remove();
        }

        const line = document.createElement("div");
        line.className = "mb-1";
        line.dataset.logId = String(log.id || "");
        line.innerHTML = "";

        const time = document.createElement("span");
        time.className = "text-secondary";
        time.textContent = `${formatTime(log.createdAt)} `;

        const level = document.createElement("span");
        level.className = "text-info";
        level.textContent = `[${log.logLevel || "Info"}] `;

        const source = document.createElement("span");
        source.className = "text-warning";
        source.textContent = `[${log.source || "process"}] `;

        const message = document.createElement("span");
        message.textContent = log.message || "";

        line.appendChild(time);
        line.appendChild(level);
        line.appendChild(source);
        line.appendChild(message);

        logStream.appendChild(line);
        logStream.scrollTop = logStream.scrollHeight;
    }

    function updateStopButton(run) {
        if (!stopButton) {
            return;
        }

        const canStop = run.canStop && run.status !== "Stopping";
        stopButton.disabled = !canStop;
        if (!canStop) {
            stopButton.textContent = run.status === "Stopping" ? "Parando..." : "Crawler finalizado";
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

    function setText(id, value) {
        const element = document.getElementById(id);
        if (!element) {
            return;
        }

        element.textContent = value ?? "";
    }

    function setVisibility(id, visible) {
        const element = document.getElementById(id);
        if (!element) {
            return;
        }

        element.classList.toggle("d-none", !visible);
    }

    function formatDate(value) {
        if (!value) {
            return "-";
        }

        return new Date(value).toLocaleString("pt-BR");
    }

    function formatTime(value) {
        if (!value) {
            return "--:--:--";
        }

        return new Date(value).toLocaleTimeString("pt-BR");
    }
})();
