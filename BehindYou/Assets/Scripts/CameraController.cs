using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Player playerOne;
    [SerializeField]
    private Player playerTwo;
    [SerializeField]
    private float cameraMoveSpeed;
    [SerializeField, Range(0, 10)]
    private float cameraZoomSpeed;
    [SerializeField]
    private float minSize;
    [SerializeField]
    private float distanceBuffer;

    private new Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        Vector2 targetPosition = 0.5f * (playerOne.Position + playerTwo.Position);
        transform.position += (Vector3)Vector2.ClampMagnitude(targetPosition - (Vector2)transform.position, cameraMoveSpeed * Time.deltaTime);

        Vector2 playerDistance = (playerOne.Position - playerTwo.Position) * distanceBuffer;
        playerDistance.x = Mathf.Abs(playerDistance.x);
        playerDistance.y = Mathf.Abs(playerDistance.y);

        float targetOrthographicSize = 0.0f;

        /*if (Mathf.Abs(playerDistance.x) > Mathf.Abs(playerDistance.y))
        {
            //-200 for the Player Stats UI elements
            float mult = (float)(camera.pixelHeight - 200) / (float)camera.pixelWidth;
            targetOrthographicSize = mult * (playerDistance.x / 2);

            Debug.Log("Mult: " + mult + ", Height: " + camera.pixelHeight + ", Width: " + camera.pixelWidth);
        }
        else
        {
            targetOrthographicSize = playerDistance.y / 2;
        }*/

        targetOrthographicSize = playerDistance.magnitude / 2;

        if(targetOrthographicSize < minSize)
        {
            targetOrthographicSize = minSize;
        }

        float orthoChange = targetOrthographicSize - camera.orthographicSize;

        if(Mathf.Abs(orthoChange) > camera.orthographicSize * cameraZoomSpeed * Time.deltaTime)
        {
            orthoChange = camera.orthographicSize * cameraZoomSpeed * Time.deltaTime * (orthoChange / Mathf.Abs(orthoChange));
        }

        camera.orthographicSize += orthoChange;
    }
}
