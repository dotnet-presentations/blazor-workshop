export function getLocalStorageItem(key) {
    return key in localStorage ? JSON.parse(localStorage[key]) : null;
}

export function setLocalStorageItem(key, value) {
    localStorage[key] = JSON.stringify(value);
}

export function deleteLocalStorageItem(key) {
    delete localStorage[key];
}
