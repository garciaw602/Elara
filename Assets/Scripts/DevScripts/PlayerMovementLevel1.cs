using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementLevel1 : MonoBehaviour
{
    public float moveSpeed = 5f;  // Velocidad de movimiento
    public Transform cameraTransform; // Transform de la c�mara (se asignar� en el inspector)

    private Vector3 moveDirection;

    void Update()
    {
        MovePlayer();
        FollowCamera();
    }

    // Funci�n que mueve al jugador
    void MovePlayer()
    {
        // Capturamos la entrada del teclado (W, A, S, D o las teclas de flecha)
        float horizontal = Input.GetAxis("Horizontal");  // A/D o flechas izquierda/derecha
        
        float vertical = Input.GetAxis("Vertical");      // W/S o flechas arriba/abajo

        // Creamos la direcci�n de movimiento basada en las teclas
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Movemos al jugador en la direcci�n deseada
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    // Funci�n que hace que la c�mara siga al jugador
    void FollowCamera()
    {
        if (cameraTransform != null)
        {
            cameraTransform.position = new Vector3(transform.position.x, cameraTransform.position.y, transform.position.z);
        }
    }
}
