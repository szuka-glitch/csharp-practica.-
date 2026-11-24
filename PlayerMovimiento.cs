using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovimiento : MonoBehaviour
{
    // --- ESTO ES LO NUEVO QUE FALTABLE ---
    public static PlayerMovimiento Instance;

    [Header("Referencias")]
    [SerializeField] private Transform camara;
    private CharacterController controlador;

    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 5f;
    [SerializeField] private float velocidadCorrer = 8f;

    [SerializeField] private float estaminaMax = 100f;
    [SerializeField] private float consumoPorSegundo = 18f;
    [SerializeField] private float recuperacionPorSegundo = 9f;
    [SerializeField] private float delayRecuperacion = 4.0f;
    [SerializeField] private Image barraEstaminaFill;
    private float estaminaActual;
    private float tiempoSinCorrer = 3f;

    [Header("Gravedad")]
    [SerializeField] private float Gravedad = -9f;
    private Vector3 velocidadVertical;
    public bool freeze;

    private void Awake()
    {
        // --- CONFIGURACIÓN DEL SINGLETON ---
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        controlador = GetComponent<CharacterController>();

        if (camara == null && Camera.main != null)
            camara = Camera.main.transform;

        estaminaActual = estaminaMax;
    }

    void Update()
    {
        if (!freeze)
        {
            MoverJugadorEnPlano();
            AplicarGravedad();
        }
    }

    private void MoverJugadorEnPlano()
    {
        float ValorHorizontal = Input.GetAxisRaw("Horizontal");
        float ValorVertical = Input.GetAxisRaw("Vertical");

        Vector3 adelanteCamara = camara.forward;
        Vector3 derechaCamara = camara.right;

        adelanteCamara.y = 0f;
        derechaCamara.y = 0f;

        adelanteCamara.Normalize();
        derechaCamara.Normalize();

        Vector3 direccionplano = (derechaCamara * ValorHorizontal + adelanteCamara * ValorVertical);

        if (direccionplano.sqrMagnitude > 0.0001f)
            direccionplano.Normalize();

        bool seEstaMoviendo = direccionplano.sqrMagnitude > 0.0001f;
        bool botonCorrer = Input.GetKey(KeyCode.LeftShift);
        bool puedoCorrer = estaminaActual > 0.01f;
        bool corriendo = botonCorrer && seEstaMoviendo && puedoCorrer;

        if (corriendo)
        {
            estaminaActual -= consumoPorSegundo * Time.deltaTime;
            tiempoSinCorrer = 0f;
        }
        else
        {
            tiempoSinCorrer += Time.deltaTime;
            if (tiempoSinCorrer >= delayRecuperacion)
            {
                estaminaActual += recuperacionPorSegundo * Time.deltaTime;
            }
        }

        estaminaActual = Mathf.Clamp(estaminaActual, 0f, estaminaMax);

        if (barraEstaminaFill != null)
        {
            barraEstaminaFill.fillAmount = estaminaActual / estaminaMax;
        }

        float velocidadActual = corriendo ? velocidadCorrer : velocidadMovimiento;

        Vector3 desplazamientoXZ = direccionplano * (velocidadActual * Time.deltaTime);

        controlador.Move(desplazamientoXZ);
    }

    private void AplicarGravedad()
    {
        velocidadVertical.y += Gravedad * Time.deltaTime;
        controlador.Move(velocidadVertical * Time.deltaTime);

        if (controlador.isGrounded && velocidadVertical.y < 0)
        {
            velocidadVertical.y = -2f;
        }
    }
}