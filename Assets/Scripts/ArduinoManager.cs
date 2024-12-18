using System.Collections;
using System.Collections.Generic;
using Uduino;
using UnityEngine;

public class ArduinoManager : MonoBehaviour
{
    private const float TIME_BEFORE_LOW = 0.02f;

    // This method is called when the script instance is being loaded
    void Awake()
    {
        // Initialize the Uduino plugin
        UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
        UduinoManager.Instance.OnBoardDisconnected += OnBoardDisconnected;
    }

    // This method is called when the script instance is being destroyed
    void OnDestroy()
    {
        // Clean up the Uduino plugin
        UduinoManager.Instance.OnBoardConnected -= OnBoardConnected;
        UduinoManager.Instance.OnBoardDisconnected -= OnBoardDisconnected;
    }

    // This method is called when an Arduino board is connected
    void OnBoardConnected(UduinoDevice board)
    {
        Debug.Log("Arduino connected: " + board.name);
    }

    // This method is called when an Arduino board is disconnected
    void OnBoardDisconnected(UduinoDevice board)
    {
        Debug.Log("Arduino disconnected: " + board.name);
    }

    // Start is called before the first frame update
    void Start()
    {
        UduinoManager.Instance.digitalWrite(0, State.LOW);
        UduinoManager.Instance.digitalWrite(1, State.LOW);
        UduinoManager.Instance.digitalWrite(2, State.LOW);
        UduinoManager.Instance.digitalWrite(3, State.LOW);
        UduinoManager.Instance.digitalWrite(4, State.LOW);
        UduinoManager.Instance.digitalWrite(5, State.LOW);
        UduinoManager.Instance.digitalWrite(6, State.LOW);
        UduinoManager.Instance.digitalWrite(7, State.LOW);
        UduinoManager.Instance.digitalWrite(8, State.LOW);
        UduinoManager.Instance.digitalWrite(9, State.LOW);
        UduinoManager.Instance.digitalWrite(10, State.LOW);
        UduinoManager.Instance.digitalWrite(11, State.LOW);
        UduinoManager.Instance.digitalWrite(12, State.LOW);
        UduinoManager.Instance.digitalWrite(13, State.LOW);
    }

    public void SetPinHigh(int pinNb)
    {
        StartCoroutine(SetPinHighCoroutine(pinNb));
    }

    public void SetPinAndStayHigh(int pinNb)
    {
        StartCoroutine(SetPinAndStayHighCoroutine(pinNb));
    }

    public void SetPinHighAndChoseWhenLow(int pinNb, float lowTime)
    {
        StartCoroutine(SetPinHighAndChoseWhenLowCoroutine(pinNb, lowTime));
    }

    public void SetMultiplePinsHigh(List<int> pinNbs)
    {
        StartCoroutine(SetMultiplePinsHighCoroutine(pinNbs));
    }

    public void SetMultiplePinsHighBinary(int nb)
    {
        StartCoroutine(SetMultiplePinsHighBinaryCoroutine(nb));
    }

    private IEnumerator SetMultiplePinsHighCoroutine(List<int> pinNbs)
    {
        foreach (int pinNb in pinNbs)
        {
            UduinoManager.Instance.digitalWrite(pinNb, State.HIGH);
        }
        yield return new WaitForSeconds(TIME_BEFORE_LOW);
        foreach (int pinNb in pinNbs)
        {
            UduinoManager.Instance.digitalWrite(pinNb, State.LOW);
        }
        yield return null;
    }

    private IEnumerator SetMultiplePinsHighBinaryCoroutine(int nb)
    {
        List<int> pinNbs = new List<int>();

        // Convert the number to binary and set the corresponding pins high
        string binary = System.Convert.ToString(nb, 2);
        for (int i = 0; i < binary.Length; i++)
        {
            if (binary[binary.Length - i - 1] == '1')
            {
                UduinoManager.Instance.digitalWrite(i, State.HIGH);
                pinNbs.Add(i);
            }
        }
        Debug.Log("Pin numbers: " + string.Join(", ", pinNbs.ToArray()) + " are set high.");
        yield return new WaitForSeconds(TIME_BEFORE_LOW);

        for (int i = 0; i < pinNbs.Count; i++)
        {
            UduinoManager.Instance.digitalWrite(pinNbs[i], State.LOW);
        }
        yield return null;
    }

    private IEnumerator SetPinHighCoroutine(int pinNb)
    {
        UduinoManager.Instance.digitalWrite(pinNb, State.HIGH);
        yield return new WaitForSeconds(TIME_BEFORE_LOW);
        UduinoManager.Instance.digitalWrite(pinNb, State.LOW);
    }

    private IEnumerator SetPinAndStayHighCoroutine(int pinNb)
    {
        UduinoManager.Instance.digitalWrite(pinNb, State.HIGH);
        yield return null;
    }

    private IEnumerator SetPinHighAndChoseWhenLowCoroutine(int pinNb, float lowTime)
    {
        UduinoManager.Instance.digitalWrite(pinNb, State.HIGH);
        yield return new WaitForSeconds(lowTime);
        UduinoManager.Instance.digitalWrite(pinNb, State.LOW);

        yield return null;
    }
}
