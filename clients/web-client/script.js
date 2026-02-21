
var connectionUrlInput = document.getElementById("connectionUrl")
var connectButton = document.getElementById("connectionButton")
var statelabel = document.getElementById("stateLabel");
var sendMessageInput = document.getElementById("sendMessage");
var sendButton = document.getElementById("sendButton");
var closeButton = document.getElementById("closeButton");
var recipents = document.getElementById("recipents");
var commsLog = document.getElementById("commsLog");
var connIDLabel = document.getElementById("connIDLabel");

// Custom functions 
const htmlEscape = function (str) {
    console.log(str);
    return str.toString()
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
}
const updateState = function () {
    connectionUrlInput.disable = true;
    connectButton.disable = true;
    if (!socket) {
        handleState()
    } else {
        switch (socket.readyState) {
            case WebSocket.CLOSED:
                statelabel.innerHTML = 'Closed'
                connIDLabel.innerHTML = 'ConnID : N/a'
                handleState(true)
                connectionUrlInput.disable = false;
                connectButton.disable = false;
                break;
            case WebSocket.CLOSING:
                statelabel.innerHTML = 'Closing'
                handleState(true)
                break;
            case WebSocket.CONNECTING:
                statelabel.innerHTML = 'Connectiong ...'
                handleState(false);
                break;
            case WebSocket.OPEN:
                statelabel.innerHTML = 'Open';
                handleState(false)
                break;
            default:
                statelabel.innerHTML = `Unknown WebSocket State: ${htmlEscapte(socke.readyState)}`
        }
    }
}
const handleState = function (state) {
    sendMessageInput.disable = state;
    sendButton.disable = state;
    closeButton.disable = state;
    recipents.disable = state;
}

connectionUrlInput.value = "ws://localhost:5000";

connectButton.onclick = () => {
    statelabel.innerHTML = 'Attempting to connect ...';
    socket = new WebSocket(connectionUrlInput.value);
    // open socket
    socket.onopen = (event) => {
        updateState();
        commsLog.innerHTML += `<tr> 
                <td colspan = "3"> Connection Opened </td> 
                </tr>`;
    };
    setTimeout(socket.onclose = (event) => {
        updateState();
        const reason = event.reason ? htmlEscape(event.reason) : "No reason provided";
        commsLog.innerHTML += `<tr> 
                <td colspan = "3"> Connection Closed: ${htmlEscape(event.code)} 
                    Reason: ${reason}</td> 
                </tr>`;
    }, 5000);
    //close socket
    
    // update on error
    socket.onerror = updateState();
    socket.onmessage = (event) => {
        commsLog.innerHTML += `<tr> 
                <td> Server </td> 
                <td> Client </td> 
                <td> ${htmlEscape(event.data)} </td> 
                </tr>`;
    }

}
