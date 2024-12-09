using UnityEngine;
using System.Collections; // Required for IEnumerator
using System.Collections.Generic; // Required for List<T>, etc.
using System.Linq;
using System;

public class InputDebugger : MonoBehaviour
{
    public bool testMode=false;
    public bool newInput=false;
    private string[] connectedJoysticks; // Almacena los nombres de los mandos conectados
    private List<string> previousJoysticks = new List<string>();


    void Start()
    {
        StartCoroutine(CheckJoysticksPeriodically());
    }

    void Update(){   
        if (testMode) {
            // Debug de los controles de los jugadores, si se pulsa un boton desconocido se mostrara como se llama
            for (int i = 1; i <= 4; i++) {
                if (Input.GetAxis($"Horizontal{i}") != 0)
                    Debug.Log($"Horizontal{i}: {Input.GetAxis($"Horizontal{i}")}");
                if (Input.GetAxis($"Vertical{i}") != 0)
                    Debug.Log($"Vertical{i}: {Input.GetAxis($"Vertical{i}")}");
                if (Input.GetButtonDown($"Jump{i}"))
                    Debug.Log($"Jump{i}");
                if (Input.GetButtonDown($"Crouch{i}"))
                    Debug.Log($"Crouch{i}");
                if (Input.GetButtonDown($"Start{i}"))
                    Debug.Log($"Start{i}");
                if (i != 1) {
                    if (Input.GetButtonDown($"Fire1_{i}"))
                        Debug.Log($"Fire1_{i}");
                    if (Input.GetButtonDown($"Fire2_{i}"))
                        Debug.Log($"Fire2_{i}");
                } else if (i == 1) {
                    if (Input.GetButtonDown("Fire1"))
                        Debug.Log("Fire1");
                    if (Input.GetButtonDown("Fire2"))
                        Debug.Log("Fire2");
                }
        
                // Verificar botones sin nombre
                for (int j = 0; j < 20 && newInput; j++) { // Asumiendo que hay un máximo de 20 botones
                    if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), $"Joystick{i}Button{j}"))) {
                        Debug.Log($"Joystick{i}Button{j}");
                    }
                }
            }
        }
    }


    

    private IEnumerator CheckJoysticksPeriodically()
    {
        while (true)
        {
            UpdateJoystickConnections();
            yield return new WaitForSeconds(5f);
        }
    }


    private void UpdateJoystickConnections()
    {
        var joysticks = Input.GetJoystickNames();
        var currentJoysticks = new List<string>(joysticks);
        
        // Comparar si los joysticks han cambiado
        if (!Enumerable.SequenceEqual(currentJoysticks, previousJoysticks))
        {
            Debug.Log("Joysticks conectados:");
            for (int i = 0; i < joysticks.Length; i++)
            {
                if (!string.IsNullOrEmpty(joysticks[i]))
                    Debug.Log($"Joystick {i + 1}: {joysticks[i]}");
            }
            previousJoysticks = currentJoysticks; // Actualizar lista previa
            AssignJoysticks();
        }
    }

    private void AssignJoysticks()
    {
        var joysticks = Input.GetJoystickNames();

        string joystick1 = null;
        string joystick2 = null;

        int assignedCount = 0;

        for (int i = 0; i < joysticks.Length; i++)
        {
            if (!string.IsNullOrEmpty(joysticks[i]))
            {
                if (assignedCount == 0)
                {
                    joystick1 = joysticks[i];
                    Debug.Log($"Joystick 1 asignado a: {joystick1}");
                }
                else if (assignedCount == 1)
                {
                    joystick2 = joysticks[i];
                    Debug.Log($"Joystick 2 asignado a: {joystick2}");
                }
                assignedCount++;
            }

            if (assignedCount >= 2) break;
        }

        // Log de información si no hay suficientes joysticks
        if (joystick1 == null)
            Debug.Log("No hay joystick asignado a Joystick 1");
        if (joystick2 == null)
            Debug.Log("No hay joystick asignado a Joystick 2");
    }

}

