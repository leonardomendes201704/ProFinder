(function () {
    const form = document.getElementById("region-form");
    if (!form) {
        return;
    }

    const lookupUrl = form.dataset.regionLookupUrl;
    const stateSelect = document.getElementById("Input_State");
    const citySelect = document.getElementById("Input_City");
    const neighborhoodSelect = document.getElementById("Input_Neighborhood");
    const zipCodeInput = document.getElementById("Input_ZipCode");
    const latitudeInput = document.getElementById("Input_Latitude");
    const longitudeInput = document.getElementById("Input_Longitude");

    stateSelect?.addEventListener("change", async () => {
        citySelect.dataset.selectedValue = "";
        neighborhoodSelect.dataset.selectedValue = "";
        await loadCities(stateSelect.value, "");
        clearSelect(neighborhoodSelect, "Selecione um bairro");
    });

    citySelect?.addEventListener("change", async () => {
        neighborhoodSelect.dataset.selectedValue = "";
        const selectedCityId = citySelect.selectedOptions[0]?.dataset.cityId ?? "";
        await loadDistricts(selectedCityId, "");
    });

    zipCodeInput?.addEventListener("blur", async () => {
        const normalizedZipCode = normalizeZipCode(zipCodeInput.value);
        if (normalizedZipCode.length !== 8) {
            return;
        }

        zipCodeInput.value = formatZipCode(normalizedZipCode);

        try {
            const response = await fetch(`${lookupUrl}?handler=ZipCode&zipCode=${encodeURIComponent(normalizedZipCode)}`);
            if (!response.ok) {
                return;
            }

            const result = await response.json();
            if (!result || !result.stateCode) {
                return;
            }

            stateSelect.value = result.stateCode;
            citySelect.dataset.selectedValue = result.cityName ?? "";
            neighborhoodSelect.dataset.selectedValue = result.districtName ?? "";

            await loadCities(result.stateCode, result.cityName, result.cityId);

            const resolvedCityId = citySelect.selectedOptions[0]?.dataset.cityId ?? result.cityId ?? "";
            await loadDistricts(resolvedCityId, result.districtName);

            if (result.latitude !== null && result.latitude !== undefined) {
                latitudeInput.value = result.latitude;
            }

            if (result.longitude !== null && result.longitude !== undefined) {
                longitudeInput.value = result.longitude;
            }
        } catch (error) {
            console.error("Erro ao consultar o CEP.", error);
        }
    });

    initialize();

    async function initialize() {
        const selectedState = stateSelect?.value ?? "";
        const selectedCity = citySelect?.dataset.selectedValue || citySelect?.value || "";
        const selectedNeighborhood = neighborhoodSelect?.dataset.selectedValue || neighborhoodSelect?.value || "";

        if (!selectedState) {
            return;
        }

        await loadCities(selectedState, selectedCity);
        const selectedCityId = citySelect.selectedOptions[0]?.dataset.cityId ?? "";
        await loadDistricts(selectedCityId, selectedNeighborhood);
    }

    async function loadCities(stateCode, selectedCityName, selectedCityId) {
        clearSelect(citySelect, "Selecione uma cidade");
        clearSelect(neighborhoodSelect, "Selecione um bairro");

        if (!stateCode) {
            return;
        }

        try {
            const response = await fetch(`${lookupUrl}?handler=Cities&stateCode=${encodeURIComponent(stateCode)}`);
            if (!response.ok) {
                return;
            }

            const cities = await response.json();
            appendOptions(citySelect, cities, "name", selectedCityName, selectedCityId);
        } catch (error) {
            console.error("Erro ao carregar cidades.", error);
        }
    }

    async function loadDistricts(cityId, selectedDistrictName) {
        clearSelect(neighborhoodSelect, "Selecione um bairro");

        if (!cityId) {
            if (selectedDistrictName) {
                neighborhoodSelect.add(new Option(selectedDistrictName, selectedDistrictName, true, true));
            }

            return;
        }

        try {
            const response = await fetch(`${lookupUrl}?handler=Districts&cityId=${encodeURIComponent(cityId)}`);
            if (!response.ok) {
                return;
            }

            const districts = await response.json();
            appendOptions(neighborhoodSelect, districts, "name", selectedDistrictName);
        } catch (error) {
            console.error("Erro ao carregar bairros.", error);
        }
    }

    function appendOptions(select, items, valueProperty, selectedValue, selectedId) {
        if (!select) {
            return;
        }

        const normalizedSelectedId = selectedId ? String(selectedId) : "";
        const normalizedSelectedValue = selectedValue ?? "";

        for (const item of items) {
            const option = new Option(item[valueProperty], item[valueProperty], false, item[valueProperty] === normalizedSelectedValue);
            if (item.id !== undefined && item.id !== null) {
                option.dataset.cityId = String(item.id);
            }

            if (normalizedSelectedId && String(item.id) === normalizedSelectedId) {
                option.selected = true;
            }

            select.add(option);
        }

        if (normalizedSelectedValue && !Array.from(select.options).some(option => option.value === normalizedSelectedValue)) {
            const option = new Option(normalizedSelectedValue, normalizedSelectedValue, true, true);
            select.add(option);
        }
    }

    function clearSelect(select, placeholder) {
        if (!select) {
            return;
        }

        select.innerHTML = "";
        select.add(new Option(placeholder, ""));
    }

    function normalizeZipCode(value) {
        return (value ?? "").replace(/\D/g, "");
    }

    function formatZipCode(value) {
        return value.length === 8 ? `${value.slice(0, 5)}-${value.slice(5)}` : value;
    }
})();
