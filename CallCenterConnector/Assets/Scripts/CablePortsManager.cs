using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CablePortsManager : MonoBehaviour
{
    [SerializeField] private float countDownTime = 30f;
    [SerializeField] private GameObject LossCanvas;
    [SerializeField] private GameObject WinCanvas;
    [SerializeField] private AudioSource BGM;
    [SerializeField] private AudioClip _2LivesMusic;
    [SerializeField] private AudioClip _1LiveMusic;
    [SerializeField] private AudioSource ambianceSound;
    public float currentTime;
    public int lives = 3;
    public CablePort[] ports;
    private int numberSelected = 0;
    public int numberOccupied = 0;

    public void Start()
    {
        //get all ports when game starts
        ports = FindObjectsOfType<CablePort>();
        Debug.Log(ports.Length);
        StartCoroutine(ActivatePorts());
    }

    public CablePort SelectClosestPort(CablePort myPort)
    {
        float min = float.MaxValue;
        int minIndex = -1;
        CablePort closestPort;

        for (int i = 0; i < ports.Length; i++)
        {
            if (ports[i] != myPort && !ports[i].wasSelected)
            {
                if (Vector3.Distance(myPort.transform.position, ports[i].transform.position) < min)
                {
                    minIndex = i;
                    min = Vector3.Distance(myPort.transform.position, ports[i].transform.position);
                }
            }
            if (minIndex == -1 && i == ports.Length - 1)
            {
                Debug.Log("Not enough ports in scene");
                return null;
            }
        }

        closestPort = ports[minIndex];
        Debug.Log(minIndex);
        closestPort.wasSelected = true;
        closestPort.myRenderer.material = closestPort.selectedMaterial;
        numberSelected++;

        return closestPort;
    }

    public CablePort SelectRandomPort()
    {
        CablePort myPort = ports[UnityEngine.Random.Range(0, ports.Length)];
        int index = -1;
        // avoid selecting the same port multiple times.
        while (myPort.wasSelected)
        {
            index = UnityEngine.Random.Range(0, ports.Length);
            myPort = ports[index];
        }
        myPort.myRenderer.material = myPort.selectedMaterial;
        myPort.portSFX.Play();
        if (myPort.portSFX.isPlaying)
        {
            Debug.Log(myPort.portSFX.volume);
            Debug.Log("Audio Played");
        }
        else
        {
            Debug.Log(myPort.portSFX.clip.name);
            Debug.Log("Audio failed to play");
        }
        myPort.wasSelected = true;
        numberSelected++;

        return myPort;
    }

    public IEnumerator ActivatePorts()
    {
        CablePort port1;
        CablePort port2;

        while (numberSelected <= ports.Length)
        {
            if(numberSelected == ports.Length)
            {
                GameWon();
                yield break;
            }
            //have an even number of ports in the scene
            port1 = SelectRandomPort();
            port2 = SelectClosestPort(port1);
            if (port1 == null || port2 == null)
            {
                Debug.Log("Not enough ports (Activate Ports)");
                yield break;

            }
            Debug.Log("Ports activated!");
            currentTime = countDownTime;

            while (currentTime >= 0)
            {
                yield return new WaitForSeconds(1f);
                currentTime--;
                Debug.Log("Countdown: " + currentTime);
                if (port1.portOccupied && port2.portOccupied)
                {
                    Debug.Log("Attached successfully! :D");
                    numberOccupied++;
                    break;
                }
                else if(currentTime == 0 && (!port1.portOccupied || !port2.portOccupied))
                {
                    lives--;
                    if(lives == 2)
                    {
                        Debug.Log("2 lives left");
                        BGM.clip = _2LivesMusic;
                        BGM.Play();
                    }
                    if(lives == 1)
                    {
                        Debug.Log("1 life left");
                        BGM.clip = _1LiveMusic;
                        BGM.Play();
                    }
                    if(lives == 0)
                    {
                        port1.portSFX.Stop();
                        port2.portSFX.Stop();
                        GameOver();
                        yield break;
                    }
                }
            }
        }
    }
    public void GameOver()
    {
        BGM.Stop();
        LossCanvas.SetActive(true);
        Debug.Log("Game over :-(");
    }

    public void GameWon()
    {
        BGM.Stop();
        WinCanvas.SetActive(true);
        Debug.Log("All Ports occupied. You win! :D");
    }
}
