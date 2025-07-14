using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementLevel1 : MonoBehaviour
{
    public float moveSpeed = 5f;  // Velocidad de movimiento
    public Transform cameraTransform; // Transform de la cámara (se asignará en el inspector)

    private Vector3 moveDirection;

    void Update()
    {
        MovePlayer();
        FollowCamera();
    }

    // Función que mueve al jugador
    void MovePlayer()
    {
        // Capturamos la entrada del teclado (W, A, S, D o las teclas de flecha)
        float horizontal = Input.GetAxis("Horizontal");  // A/D o flechas izquierda/derecha
        
        float vertical = Input.GetAxis("Vertical");      // W/S o flechas arriba/abajo

        // Creamos la dirección de movimiento basada en las teclas
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Movemos al jugador en la dirección deseada
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    // Función que hace que la cámara siga al jugador
    void FollowCamera()
    {
        if (cameraTransform != null)
        {
            cameraTransform.position = new Vector3(transform.position.x, cameraTransform.position.y, transform.position.z);
        }
    }
}
