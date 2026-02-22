
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
    connectionUrlInput.disabled = true;
    connectButton.disabled = true;
    if (!socket) {
        handleState()
    } else {
        switch (socket.readyState) {
            case WebSocket.CLOSED:
                statelabel.innerHTML = 'Closed'
                connIDLabel.innerHTML = 'ConnID : N/a'
                handleState(true)
                connectionUrlInput.disabled = false;
                connectButton.disabled = false;
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
                connIDLabel.innerHTML = `ConnID: unknown`
                handleState(false);
                break;
            default:
                statelabel.innerHTML = `Unknown WebSocket State: ${htmlEscape(socke.readyState)}`
                handleState(true);
                break;
        }
    }
}
const handleState = function (state) {
    sendMessageInput.disabled = state;
    sendButton.disabled = state;
    closeButton.disabled = state;
    recipents.disabled = state;
}

const setConnectionId = (id)=>{
    connIDLabel.innerHTML = `ConnID: ${id}`
    return ;
}

connectionUrlInput.value = "ws://localhost:5000";
let socket = null;

connectButton.onclick = () => {
    console.log("Attempting to connect ...")
    socket = new WebSocket(connectionUrlInput.value);

    statelabel.innerHTML = 'Attempting to connect ...';
    // open socket
    socket.onopen = (event) => {
        updateState();
        commsLog.innerHTML += `<tr> 
                <td colspan = "3"> Connection Opened </td> 
                </tr>`;
    };
    socket.onclose = (event) => {
        updateState();
        let reason;
        if(!event.reason){
            reason = htmlEscape(event.reason);
        }else{
            reason = "No reason provided";
        }
       // const reason = event.reason ? htmlEscape(event.reason) : "No reason provided";
        commsLog.innerHTML += `<tr> 
                <td colspan = "3"> Connection Closed: ${htmlEscape(event.code)} 
                    Reason: ${reason}</td> 
                </tr>`;
    };
    //close socket
    
    // update on error
    socket.onerror = updateState;
    socket.onmessage = (event) => {
        var content = JSON.parse(event.data);
        console.log(content);
        if(content.type === 'broadcast'){
            var line = document.createElement("tr");
            var from =document.createElement("td");
            var to = document.createElement("td");
            var message = document.createElement("td");
            var date = document.createElement("td");
            var type = document.createElement("td");

            from.textContent = content.connectionId;
            to.textContent = "Client";
            message.textContent = content.message;
            date.textContent = content.timestamp;
            type.textContent = content.type;

            line.appendChild(from);
            line.appendChild(to);
            line.appendChild(message);
            line.appendChild(date);
            line.appendChild(type);

            commsLog.appendChild(line);
            return;
        }
        if(content.connectionId){
            setConnectionId(content.connectionId);
            return;
        }
        commsLog.innerHTML += `<tr> 
            <td> Server </td> 
            <td> Client </td> 
            <td> ${htmlEscape(event.data)} </td> 
        </tr>`;

    }

}

closeButton.onclick = ()=>{
    if(!socket || socket.readyState !== WebSocket.OPEN){
        alert("Socket not connected");
        return;
    }
    socket.close(1000, "Closing from client");
    sendMessageInput.value= "";
}

sendButton.onclick = () =>{
     if(!socket || socket.readyState !== WebSocket.OPEN){
        alert("Socket not connected");
        return;
    }
    var data = sendMessageInput.value;
    socket.send(data)
    alert('Message sent to server')
    var line = document.createElement('tr');
    var cell1 = document.createElement('td');
    var cell2 = document.createElement('td');
    var cell3 = document.createElement('td');
    cell1.textContent = 'Server';
    cell2.textContent = "Client";
    cell3.textContent = `${htmlEscape(data)}`;
    line.appendChild(cell1)
    line.appendChild(cell2)
    line.appendChild(cell3)
    commsLog.appendChild(line);
    
}
