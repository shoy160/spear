namespace Spear.Protocol.WebSocket
{
    public static class SimpleHtmlClient
    {
        public const string HTML =
@"<!DOCTYPE html>
  <meta charset=""utf-8""/>
  <title>WebSocket Echo/Broadcast Client</title>
  <script language=""javascript"" type=""text/javascript"">
  var wsUri = ""ws://localhost:8080/"";
        var output;
        var websocket;
        function init()
        {
            output = document.getElementById(""output"");
            configWebSocket();
        }
        function configWebSocket()
        {
            websocket = new WebSocket(wsUri);
            websocket.onopen = function(evt) { onOpen(evt) };
            websocket.onclose = function(evt) { onClose(evt) };
            websocket.onmessage = function(evt) { onMessage(evt) };
            websocket.onerror = function(evt) { onError(evt) };
        }
        function onOpen(evt)
        {
            emit(""SOCKET OPENED"");
            sendTextFrame(""Hello"");
        }
        function onClose(evt)
        {
            emit(""SOCKET CLOSED"");
        }
        function onMessage(evt)
        {
            emit('<span style=""color:blue;"">RECEIVED: ' + evt.data + '</span>');
        }
        function onError(evt)
        {
            emit('<span style=""color:red;"">ERROR: ' + evt.data + '</span>');
        }
        function sendTextFrame(message)
        {
            if (websocket.readyState == WebSocket.OPEN)
            {
                emit(""SENT: "" + message);
                websocket.send(message);
            }
            else
            {
                emit(""Socket not open, state: "" + websocket.readyState);
            }
        }
        function emit(message)
        {
            var pre = document.createElement(""p"");
            pre.style.wordWrap = ""break-word"";
            pre.innerHTML = message;
            output.appendChild(pre);
        }
        function clickSend()
        {
            var txt = document.getElementById(""newMessage"");
            if (txt.value.length > 0)
            {
                sendTextFrame(txt.value);
                txt.value = """";
                txt.focus();
            }
        }
        function clickClose()
        {
            if (websocket.readyState == WebSocket.OPEN)
            {
                websocket.close();
            }
            else
            {
                emit(""Socket not open, state: "" + websocket.readyState);
            }
            document.getElementById(""sender"").disabled = true;
            document.getElementById(""closer"").disabled = true;
            document.getElementById(""newMessage"").disabled = true;
        }
        window.addEventListener(""load"", init, false);
  </script>
  <h2>Multi-Client WebSocket Echo/Broadcast Test</h2>
  <p><input type=""input"" id=""newMessage"" onkeyup=""if(event.key==='Enter') clickSend()""/> <input type=""button"" id=""sender"" value=""Send"" onclick=""clickSend()""/> <input type=""button"" id=""closer"" value=""Disconnect"" onclick=""clickClose()""/>
  <div id= ""output""></div> 
";
    }
}
