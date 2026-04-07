using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Chat;
using UnityEngine.UI;
using TMPro;
using Grpc.Core;


public class GrpcClient : MonoBehaviour
{
    [Header("UI")]
    public InputField inputField;
    public Button sendButton;
    public Transform chatContent;
    public GameObject chatMessagePrefab;

    private GrpcChannel channel;
    private ChatService.ChatServiceClient client;
    private AsyncDuplexStreamingCall<ChatMessage, ChatMessage> streamingCall;
    private CancellationTokenSource cts;

    private string userName = "UnityUser";

    async void Start()
    {   

        //HTTP/2 설정
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        //서버 연결
        var addr = "http://localhost:50051";
        Debug.Log(addr);

        channel = GrpcChannel.ForAddress(addr);
        client = new ChatService.ChatServiceClient(channel);

        sendButton.onClick.AddListener(OnSendButtonClick);

        await StartGrpcStreaming();
    }

    async Task StartGrpcStreaming()
    {
        cts = new CancellationTokenSource();
        streamingCall = client.Chat(cancellationToken: cts.Token);

        _ = Task.Run(async () =>
        {
            try
            {
                await foreach (var msg in streamingCall.ResponseStream.ReadAllAsync())
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        AddChatMessage(msg.User, msg.Message);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.Log($"스트림 종료: {e.Message}");
            }
        });

        Debug.Log("서버 연결 완료!");
    }

    void OnSendButtonClick()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;
        SendGrpcMessage(inputField.text);
        inputField.text = "";
    }

    async void SendGrpcMessage(string message)
    {
        var chatMsg = new ChatMessage
        {
            User = userName,
            Message = message,
            Timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        await streamingCall.RequestStream.WriteAsync(chatMsg);
    }

    void AddChatMessage(string user, string message)
    {
        var obj = Instantiate(chatMessagePrefab, chatContent);
        obj.GetComponentInChildren<TMP_Text>().text = $"[{user}]: {message}";
    }

    async void OnDestroy()
    {
        cts.Cancel();
        await streamingCall.RequestStream.CompleteAsync();
        await channel.ShutdownAsync();
    }
}
