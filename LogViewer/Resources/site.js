function formatDateTime(date) {
    const day = ('0' + date.getDate()).slice(-2);
    const month = ('0' + date.getMonth()).slice(-2);
    const year = date.getFullYear();

    const hour = ('0' + date.getHours()).slice(-2);
    const minute = ('0' + date.getMinutes()).slice(-2);

    return `${day}.${month}.${year} ${hour}:${minute}`;
}

document.querySelectorAll('time')
    .forEach(el =>
        el.innerText = formatDateTime(new Date(el.dateTime)));
