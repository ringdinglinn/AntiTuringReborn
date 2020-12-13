using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

public class ChatbotBehaviour : MonoBehaviour {

    public List<int[]> chatroomBotIndex = new List<int[]>();
    public List<ChatbotAI> chatbotAIs = new List<ChatbotAI>();

    public List<string> clientNameList = new List<string>();

    private ChatBehaviour chatBehaviour;

    public int nextSessionID;

    protected string text, response;
    protected string sessionId; // Remains null until after first message is parsed
    protected bool waiting;

    public string botid = "rabidrosie";// Name of chatbot to use. Has to be made using website
    public string appid = "una2165008"; // From Pandorabots application
    public string userkey = "2a901bdef12f158b9b6e9bd277d04766";//From Pandorabots application

    public List<Response> responses = new List<Response>();

    public NetworkManagerAT networkManager;

    private int waitToRespondRoutineCounter = 0;
    private int pandoraBotsRequestCounter = 0;

    private int messagesSentToBotCounter = 1;

    public string getResponse() {
        return response;
    }

    void Awake() {
        text = "";
        response = "Waiting for text";
        DontDestroyOnLoad(gameObject);
        CreateClientNameList();
    }

    public void GameStart() {
        InitializeChatroomBotIndex();
    }

    public void CreateClientNameList() {
        for (int i = 0; i < 100; i++) {
            clientNameList.Add("pandoraclient" + i);
        }
    }

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Chatbot Management

    private void InitializeChatroomBotIndex() {
        for (int i = 0; i < networkManager.nrChatrooms; i++) {
            int[] newArr = { -1, -1 };
            chatroomBotIndex.Add(newArr);
        }
    }

    public void ChangeChatroomBotIndex(int chatroomID, int chatbotID, bool left, bool isJoining) {
        Debug.Log("change chatroom bot index, chatroomID = " + chatroomID + ", chatbotID = " + chatbotID + ", left = " + left + ", isJoining = " + isJoining);
        int i = 0;
        if (!left) i = 1;
        chatroomBotIndex[chatroomID][i] = chatbotID;
        if (!isJoining) chatroomBotIndex[chatroomID][i] = -1;

        Debug.Log("change chatroom bot index 2, chatroomID = " + chatroomID + ", chatbotID = " + chatroomBotIndex[chatroomID][i]);

    }

    public void InitiateChabotAI() {
        foreach (ChatbotAI chatbotAI in chatbotAIs) {
            chatbotAI.GameStart();
        }
    }


    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Pandora Bot API

    string sanitizePandoraResponse(string wwwText) {
        string responseString = "";

        int startIndex = wwwText.IndexOf(" [") + 2;
        int endIndex = wwwText.IndexOf("],");
        responseString = wwwText.Substring(startIndex, endIndex - startIndex);

        return responseString;
    }


    private IEnumerator PandoraBotRequestCoRoutine(string text, int chatroomID, int sessionID, int chatbotID, string clientName, int id) {

        Debug.Log("text 1 = " + text);
        Regex rx = new Regex(@"[\.!\?]");             //make sure only one sentence is sent to pandora
        foreach (Match match in rx.Matches(text)) {
            int i = match.Index;
            text = text.Remove(i);
            break;
        }
        Debug.Log("text 2 = " + text);


        string url = "https://api.pandorabots.com/talk?botkey=RssstjtodsmGn5b1IstcJtNZI9khFR8B6xS0_Qvmtrrq5dalb0KYSIeonmRa15PUOL2I-8EtsPdp9rI_1dsWOQ~~&input=";
        url += UnityWebRequest.EscapeURL(text);
        url += "&client_name=" + clientName + "&sessionid=" + sessionID.ToString();


        UnityWebRequest wr = UnityWebRequest.Post(url, ""); //You cannot do POST with empty post data, new byte is just dummy data to solve this problem


        yield return wr.SendWebRequest();

        if (wr.error == null) {
            string r = sanitizePandoraResponse(wr.downloadHandler.text); //Where we get our chatbots response message


            rx = new Regex("(\",)");                  // Remove everything after first response
            foreach (Match match in rx.Matches(r)) {
                int i = match.Index;
                //r = r.Remove(i);
                break;
            }

            r = Regex.Replace(r, "(\", \")", " ");
            r = Regex.Replace(r, @"(\\n)", " ");
            r = Regex.Replace(r, @"(\s\s+)", " ");
            r = Regex.Replace(r, @"\\", "");
            r = Regex.Replace(r, "\"", "");

            Debug.Log(r);
            //r = r.Remove(0, 1);
            //r = r.Remove(r.Length - 1, 1);

            Response response = new Response(chatroomID, r, chatbotAIs[chatbotID].fakeName, chatbotAIs[chatbotID].playerVisualPalletID, id);
            responses.Add(response);
            SendResponseToServer(response);
            pandoraBotsRequestCounter++;
        }
        else {
            Debug.LogWarning(wr.error);
        }
    }

    public struct Response {
        public Response(int id, string t, string n, int visID, int responseID) {
            chatroomID = id;
            text = t;
            fakeName = n;
            visualPalletID = visID;
            this.id = responseID;
        }
        public int chatroomID;
        public string text;
        public string fakeName;
        public int visualPalletID;
        public int id;
    }

    public void SendTextToChatbot(string text, int chatroomID, int chatbotID, bool fromChatbot, int playerID) {
        Debug.Log("SendMessageToChatbot1");
        messagesSentToBotCounter++;
        Debug.Log("SendMessageToChatbot2");
        int sessionID = chatbotAIs[chatbotID].currentSessionID;
        string clientName = "";
        if (fromChatbot) {
            int convoPartnerID = chatroomBotIndex[chatroomID][0];
            if (convoPartnerID == chatbotID) convoPartnerID = chatroomBotIndex[chatroomID][1];
            clientName = chatbotAIs[convoPartnerID].pandoraBotsClientName;
        } else {
            clientName = clientNameList[playerID];
        }
        Debug.Log("SendMessageToChatbot6, text = " + text + ", chatroomID = " + chatroomID + ", chatbotID = " + chatbotID + ", clientName = " + clientName);
        StartCoroutine(PandoraBotRequestCoRoutine(text, chatroomID, sessionID, chatbotID, clientName, messagesSentToBotCounter));
    }

    public void SendResponseToServer(Response response) {
        //networkManager.GamePlayers[0].ReceiveMessageFromChatbot(response.text, response.chatroomID, response.fakeName, response.visualPalletID);
        //networkManager.GamePlayers[0].GetComponent<ChatBehaviour>().ChatbotSendsMessage(response.text, response.chatroomID, response.fakeName, response.visualPalletID);
        StartCoroutine(WaitToSendResponse(response));
    }

    IEnumerator WaitToSendResponse(Response response) {
        float waitTime;
        string message = response.text;
        waitTime = 0.1f * response.text.Length + response.text.Length * Random.Range(0f, 0.1f) + 1f;
        yield return new WaitForSeconds(waitTime);
        networkManager.GamePlayers[0].GetComponent<ChatBehaviour>().ChatbotSendsMessage(message, response.chatroomID, response.fakeName, response.visualPalletID);
        waitToRespondRoutineCounter++;
    }

    public void SendResponseToServerDirectly(Response response) {
        networkManager.GamePlayers[0].GetComponent<ChatBehaviour>().ChatbotSendsMessage(response.text, response.chatroomID, response.fakeName, response.visualPalletID);
    }
}
