(function () {
    const resultsContainer = document.getElementById("provider-leads-results");
    const modalElement = document.getElementById("provider-leads-map-modal");

    if (!resultsContainer || !modalElement) {
        return;
    }

    const mapUrl = resultsContainer.dataset.mapUrl;
    const detailsUrlTemplate = resultsContainer.dataset.detailsUrlTemplate || "";
    const stageElement = document.getElementById("provider-leads-map-stage");
    const canvasElement = document.getElementById("provider-leads-map-canvas");
    const summaryElement = document.getElementById("provider-leads-map-summary");
    const loadingElement = document.getElementById("provider-leads-map-loading");
    const emptyElement = document.getElementById("provider-leads-map-empty");
    const errorElement = document.getElementById("provider-leads-map-error");

    if (!mapUrl || !stageElement || !canvasElement || !summaryElement || !loadingElement || !emptyElement || !errorElement) {
        return;
    }

    if (!window.bootstrap) {
        console.error("Bootstrap nao esta disponivel para abrir o modal de mapa.");
        return;
    }

    const modal = new bootstrap.Modal(modalElement);
    let mapInstance = null;
    let markersLayer = null;
    let pendingMapView = null;
    let fitTimerId = 0;
    let requestVersion = 0;

    document.addEventListener("click", function (event) {
        const trigger = event.target.closest("[data-provider-leads-map-trigger]");
        if (!trigger) {
            return;
        }

        if (trigger.disabled) {
            return;
        }

        event.preventDefault();
        openMapModal();
    });

    modalElement.addEventListener("shown.bs.modal", function () {
        scheduleMapViewUpdate();
    });

    modalElement.addEventListener("hidden.bs.modal", function () {
        requestVersion += 1;
        pendingMapView = null;
        clearScheduledMapViewUpdate();
        destroyMap();
        setSummary("Carregando leads filtrados...");
        showLoading("Carregando leads filtrados no mapa...");
    });

    async function openMapModal() {
        const currentRequest = ++requestVersion;

        setSummary("Carregando leads filtrados...");
        showLoading("Carregando leads filtrados no mapa...");
        modal.show();

        try {
            const result = await fetchMapItems();
            if (currentRequest !== requestVersion) {
                return;
            }

            renderMapResult(result);
        } catch (error) {
            if (currentRequest !== requestVersion) {
                return;
            }

            console.error("Erro ao carregar os leads no mapa.", error);
            setSummary("Falha ao carregar o mapa.");
            showError("Nao foi possivel carregar os leads filtrados no mapa.");
        }
    }

    async function fetchMapItems() {
        const url = new URL(mapUrl, window.location.origin);
        const query = new URLSearchParams(window.location.search);
        query.delete("PageNumber");
        query.delete("PageSize");

        query.forEach(function (value, key) {
            url.searchParams.set(key, value);
        });

        const response = await fetch(url.toString(), {
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        if (!response.ok) {
            throw new Error(`Falha ao carregar os leads no mapa (${response.status}).`);
        }

        return await response.json();
    }

    function renderMapResult(result) {
        const totalFiltered = Number(result?.totalFiltered || 0);
        const totalMapped = Number(result?.totalMapped || 0);
        const items = Array.isArray(result?.items) ? result.items : [];
        const coordinateGroups = groupItemsByCoordinate(items);

        setSummary(`Exibindo ${totalMapped} lead(s) em ${coordinateGroups.length} coordenada(s) do total de ${totalFiltered} filtrado(s).`);

        if (totalFiltered === 0) {
            destroyMap();
            showEmpty("Nenhum lead encontrado com os filtros atuais.");
            return;
        }

        if (items.length === 0 || totalMapped === 0) {
            destroyMap();
            showEmpty("Nenhum lead filtrado possui coordenadas para exibicao no mapa.");
            return;
        }

        renderLeafletMap(coordinateGroups);
    }

    function renderLeafletMap(groups) {
        if (!window.L) {
            throw new Error("Leaflet nao esta disponivel.");
        }

        if (!mapInstance) {
            mapInstance = window.L.map(canvasElement, {
                zoomControl: true
            });

            window.L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                attribution: "&copy; OpenStreetMap contributors"
            }).addTo(mapInstance);

            markersLayer = window.L.layerGroup().addTo(mapInstance);
        }

        markersLayer.clearLayers();

        const bounds = [];

        groups.forEach(function (group) {
            const latitude = Number(group.latitude);
            const longitude = Number(group.longitude);

            if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) {
                return;
            }

            const marker = window.L.marker([latitude, longitude], {
                icon: buildMarkerIcon(group.items.length)
            });
            marker.bindPopup(buildPopupContent(group));
            marker.addTo(markersLayer);
            bounds.push([latitude, longitude]);
        });

        if (bounds.length === 0) {
            destroyMap();
            showEmpty("Nenhum lead filtrado possui coordenadas validas para exibicao no mapa.");
            return;
        }

        showMap();
        pendingMapView = bounds.length === 1
            ? {
                type: "single",
                center: bounds[0],
                zoom: 15
            }
            : {
                type: "bounds",
                bounds: bounds
            };
        scheduleMapViewUpdate();
    }

    function buildPopupContent(group) {
        const title = group.items.length === 1
            ? "1 lead nesta coordenada"
            : `${group.items.length} leads nesta coordenada`;
        const itemsMarkup = group.items.map(function (item) {
            const detailsUrl = detailsUrlTemplate
                ? detailsUrlTemplate.replace("__ID__", encodeURIComponent(String(item.id)))
                : "";
            const locality = item.localityDisplay || "Localidade nao informada";
            const region = item.regionDisplayName || "Regiao-alvo nao informada";
            const source = item.sourceName || "Origem nao informada";
            const nameMarkup = detailsUrl
                ? `<a href="${escapeAttribute(detailsUrl)}" class="provider-leads-map-popup-link">${escapeHtml(item.name || "Lead sem nome")}</a>`
                : escapeHtml(item.name || "Lead sem nome");

            return `
                <li class="provider-leads-map-popup-item">
                    <div class="provider-leads-map-popup-title">${nameMarkup}</div>
                    <div class="provider-leads-map-popup-meta">${escapeHtml(locality)}</div>
                    <div class="provider-leads-map-popup-meta">Regiao-alvo: ${escapeHtml(region)}</div>
                    <div class="provider-leads-map-popup-meta">Origem: ${escapeHtml(source)}</div>
                </li>
            `;
        }).join("");

        return `
            <div class="provider-leads-map-popup">
                <div class="provider-leads-map-popup-group-title">${escapeHtml(title)}</div>
                <ul class="provider-leads-map-popup-list">${itemsMarkup}</ul>
            </div>
        `;
    }

    function buildMarkerIcon(count) {
        const size = count >= 100 ? 44 : count >= 10 ? 40 : 36;

        return window.L.divIcon({
            className: "provider-leads-map-marker",
            html: `<span class="provider-leads-map-marker-badge">${escapeHtml(String(count))}</span>`,
            iconSize: [size, size],
            iconAnchor: [size / 2, size / 2],
            popupAnchor: [0, -(size / 2)]
        });
    }

    function groupItemsByCoordinate(items) {
        const groups = new Map();

        items.forEach(function (item) {
            const latitude = Number(item.latitude);
            const longitude = Number(item.longitude);

            if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) {
                return;
            }

            const key = `${latitude},${longitude}`;
            if (!groups.has(key)) {
                groups.set(key, {
                    latitude: latitude,
                    longitude: longitude,
                    items: []
                });
            }

            groups.get(key).items.push(item);
        });

        return Array.from(groups.values()).sort(function (left, right) {
            return right.items.length - left.items.length;
        });
    }

    function showLoading(message) {
        setState("loading", message);
    }

    function showEmpty(message) {
        setState("empty", message);
    }

    function showError(message) {
        setState("error", message);
    }

    function showMap() {
        setState("ready", "");
    }

    function setState(state, message) {
        loadingElement.classList.toggle("d-none", state !== "loading");
        emptyElement.classList.toggle("d-none", state !== "empty");
        errorElement.classList.toggle("d-none", state !== "error");
        canvasElement.classList.toggle("d-none", state !== "ready");

        if (state === "loading") {
            loadingElement.textContent = message;
        } else if (state === "empty") {
            emptyElement.textContent = message;
        } else if (state === "error") {
            errorElement.textContent = message;
        }
    }

    function destroyMap() {
        clearScheduledMapViewUpdate();

        if (!mapInstance) {
            return;
        }

        mapInstance.remove();
        mapInstance = null;
        markersLayer = null;
    }

    function invalidateMapSize() {
        if (!mapInstance) {
            return;
        }

        window.setTimeout(function () {
            mapInstance.invalidateSize();
        }, 0);
    }

    function scheduleMapViewUpdate() {
        if (!mapInstance || !pendingMapView || !isModalVisible()) {
            return;
        }

        clearScheduledMapViewUpdate();

        fitTimerId = window.setTimeout(function () {
            applyPendingMapView();

            fitTimerId = window.setTimeout(function () {
                applyPendingMapView();
            }, 180);
        }, 120);
    }

    function applyPendingMapView() {
        if (!mapInstance || !pendingMapView || !isModalVisible()) {
            return;
        }

        mapInstance.invalidateSize();

        if (pendingMapView.type === "single") {
            mapInstance.setView(pendingMapView.center, pendingMapView.zoom, {
                animate: false
            });
            return;
        }

        mapInstance.fitBounds(pendingMapView.bounds, {
            animate: false,
            padding: [60, 60]
        });
    }

    function clearScheduledMapViewUpdate() {
        if (!fitTimerId) {
            return;
        }

        window.clearTimeout(fitTimerId);
        fitTimerId = 0;
    }

    function isModalVisible() {
        return modalElement.classList.contains("show");
    }

    function setSummary(text) {
        summaryElement.textContent = text || "";
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function escapeAttribute(value) {
        return escapeHtml(value).replace(/`/g, "&#96;");
    }
})();
