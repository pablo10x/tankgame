using UnityEngine;

public class RTSCameraController : MonoBehaviour {
    public  float panSpeed    = 20f;
    public  float zoomSpeed   = 20f;
    public  float smoothSpeed = 10f;
    public  float minZoom     = 5f;
    public  float maxZoom     = 50f;
    public  float minAngle    = 45f; // Minimum camera angle (zoomed out)
    private float smoothedAngle;
    public  float maxAngle = 75f; // Maximum camera angle (zoomed in)

    // Border constraints
    public float minX = - 50f;
    public float maxX = 50f;
    public float minZ = - 50f;
    public float maxZ = 50f;

    private Camera  cam;
    private Vector2 lastPanPosition2;
    private Vector3 lastPanPosition3;
    private Vector3 targetPosition;
    private float   targetZoom;
    private bool    isPanning;

    private void Start() {
        cam            = Camera.main;
        targetPosition = ClampPosition(transform.position);
        targetZoom     = Mathf.Clamp(transform.position.y, minZoom, maxZoom);

        Application.targetFrameRate = 60;
    }

    private void Update() {
#if UNITY_STANDALONE || UNITY_EDITOR
        HandleMouseInput();
#elif UNITY_IOS || UNITY_ANDROID && !UNITY_EDITOR
        HandleTouchInput();
#endif

        // Smoothly move the camera towards the target position
        // Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        // smoothedPosition.y = Mathf.Lerp(transform.position.y, targetZoom, Time.deltaTime * smoothSpeed);
        // transform.position = ClampPosition(smoothedPosition);

        // Smoothly move the camera towards the target position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition,
            Time.deltaTime * smoothSpeed);
        smoothedPosition.y = Mathf.Lerp(transform.position.y, targetZoom, Time.deltaTime * smoothSpeed);
        transform.position = ClampPosition(smoothedPosition);

        // Calculate camera angle based on zoom level
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, (targetZoom - minZoom) / (maxZoom - minZoom));
        smoothedAngle      = Mathf.LerpAngle(smoothedAngle, targetAngle, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Euler(smoothedAngle, transform.rotation.y, transform.rotation.z);
    }


    public float pantimer;
    public float MAxpantimer = 0.5f;

    private void HandleMouseInput() {
        // Panning


        if (Input.GetMouseButtonDown(1)) {
            // // if (isPanning == false) {

            if (pantimer > MAxpantimer) {
                //pantimer  = 0f;
            }
            else pantimer += Time.deltaTime;

            isPanning        = true;
            lastPanPosition3 = Input.mousePosition;
            // }
        }
        else
            if (Input.GetMouseButtonUp(1)) {
                isPanning = false;
                pantimer  = 0f;
            }

        if (isPanning) {
            Vector3 delta = cam.ScreenToViewportPoint(Input.mousePosition - lastPanPosition3);
            Vector3 move  = new Vector3(- delta.x * panSpeed, 0, - delta.y * panSpeed);
            targetPosition   = ClampPosition(targetPosition + move);
            lastPanPosition3 = Input.mousePosition;
        }

        // Zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f) {
            float zoomDelta = scroll * zoomSpeed;
            float newZoom   = Mathf.Clamp(targetZoom - zoomDelta, minZoom, maxZoom);

            if (newZoom != targetZoom) {
                // Only adjust position if we're actually zooming
                Vector3 mouseWorldPos
                    = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, newZoom));
                Vector3 zoomDir = ( mouseWorldPos - transform.position ).normalized;
                targetPosition = ClampPosition(targetPosition + zoomDir * ( targetZoom - newZoom ));
                targetZoom     = newZoom;
            }
        }
    }

    private void HandleTouchInput() {
        // Panning with one finger
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                lastPanPosition2 = touch.position;
            }
            else
                if (touch.phase == TouchPhase.Moved) {
                    Vector3 delta = cam.ScreenToViewportPoint(touch.position - lastPanPosition2);
                    Vector3 move  = new Vector3(- delta.x * panSpeed, 0, - delta.y * panSpeed);
                    targetPosition   = ClampPosition(targetPosition + move);
                    lastPanPosition2 = touch.position;
                }
        }
        // Zooming with two fingers
        else
            if (Input.touchCount == 2) {
                Touch   touchZero        = Input.GetTouch(0);
                Touch   touchOne         = Input.GetTouch(1);
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos  = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude    = ( touchZeroPrevPos - touchOnePrevPos ).magnitude;
                float currentMagnitude = ( touchZero.position - touchOne.position ).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                float zoomDelta = difference * zoomSpeed * 0.01f;
                float newZoom   = Mathf.Clamp(targetZoom - zoomDelta, minZoom, maxZoom);

                if (newZoom != targetZoom) {
                    // Only adjust position if we're actually zooming
                    Vector3 touchWorldPos = cam.ScreenToWorldPoint(new Vector3(
                        ( touchZero.position.x + touchOne.position.x ) / 2,
                        ( touchZero.position.y + touchOne.position.y ) / 2,
                        newZoom));
                    Vector3 zoomDir = ( touchWorldPos - transform.position ).normalized;
                    targetPosition = ClampPosition(targetPosition + zoomDir * ( targetZoom - newZoom ));
                    targetZoom     = newZoom;
                }
            }
    }

    private Vector3 ClampPosition(Vector3 position) {
        return new Vector3(
            Mathf.Clamp(position.x, minX, maxX),
            position.y,
            Mathf.Clamp(position.z, minZ, maxZ)
        );
    }
}
