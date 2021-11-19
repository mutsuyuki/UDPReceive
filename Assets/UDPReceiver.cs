using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UniRx;
using UnityEngine.UI;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Subject<string> subject = new Subject<string>();
    [SerializeField] Text message;
    [SerializeField] Transform ballTransform;
    [SerializeField] Material ballMat;

    void Start()
    {
        ballMat.color = Color.white;

        udpClient = new UdpClient(9999);
        udpClient.BeginReceive(OnReceived, udpClient);

        subject
            .ObserveOnMainThread()
            .Subscribe(msg =>
            {
                message.text = msg;

                var data_array = msg.Split("_");
                var key = data_array[0];
                var index = data_array[1];
                var value = data_array[2];
                ballMat.color = key == "NFL" ? Color.green : Color.red;

                var x = float.Parse(index) % 3;
                x = Map(x, 0, 2, -2, 2);
                var y = Mathf.Floor(float.Parse(index) / 3f);
                y = Map(y, 0, 2, -2, 2);
                ballTransform.position = new Vector3(x, y, 0f);

                var scale = float.Parse(value);
                scale = Map(scale, 0, 100, 0, 2);
                ballTransform.localScale = new Vector3(scale, ballTransform.localScale.y, scale);
            })
            .AddTo(this);
    }

    private void OnReceived(System.IAsyncResult result)
    {
        UdpClient getUdp = (UdpClient) result.AsyncState;
        IPEndPoint ipEnd = null;

        byte[] getByte = getUdp.EndReceive(result, ref ipEnd);

        var message = Encoding.UTF8.GetString(getByte);
        subject.OnNext(message);

        getUdp.BeginReceive(OnReceived, getUdp);
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }

    public static float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}