using UnityEngine;

public class CameraScript : Photon.MonoBehaviour
{

    public CharacterMovement characterMovement;
    TerrainScript terrain;
    MainGUI maingui;
    Rect MapWindowRect = new Rect(Screen.width * 0.3f, Screen.height * 0.4f, Screen.width * 0.4f, Screen.height * 0.5f);
    bool MapWindowWindow;

    public Camera MapCamera;
    public Camera SkyBoxCamera;
    Skybox SkyboxColors;

    public float CameraUpandDownRotation;
    public float CameraUpandDownRotationSlerped;
    Transform target;
    Vector3 targetCameraDistance;
    float distance = 2.0f;
    float rotationDamping = 10.0f;
    float FieldofView; // if changing this, make sure to change on other cameras

    float PlayersRotationAngleY;
    public float PlayerHeight;
    float tempHeight = 0;
    float CameraRotationAngleY;
    public float currentHeight;
    Quaternion CameraRotation;

    GameObject Sun;
    ParticleSystem Clouds;
    Light[] DirLight;
    float x, y; // sun position
    public float alpha;
    float tempAlpha;
    public float IncreasingTime = 0;
    public float TimeElapsedSinceLastJump;

    [PunRPC]
    void SyncAlphasAndSun(float a, float ta, float Sunx, float Suny)
    {
        alpha = a;
        tempAlpha = ta;
        x = Sunx;
        y = Suny;
    }

    void ChangeSkyBoxes()
    {
        if (PhotonNetwork.isMasterClient)
            photonView.RPC("SyncAlphasAndSun", PhotonTargets.OthersBuffered, alpha, tempAlpha, x, y);

        if (alpha <= 0)
        {
            SkyboxColors.material.SetColor("_SkyTint", new Color(0, 0, 0));
            SkyboxColors.material.SetFloat("_Exposure", 0);
            var CloudColor = Clouds.main.startColor;

            CloudColor = new Color(0.15f, 0.15f, 0.15f, 0.05f);
            for (int i = 0; i < DirLight.Length; i++)
                DirLight[i].intensity = 0.15f;
            RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.15f);
            RenderSettings.fogColor = new Color(0, 0, 0);
            SkyboxColors.material.SetColor("_GroundColor", new Color(0, 0, 0));
        }
        else
        {
            //SkyboxColors.material.SetColor("_SkyTint", new Color((alpha / 200000), (alpha / 400000), 0));
            SkyboxColors.material.SetColor("_SkyTint", new Color(1, 0.5f, 0));

            SkyboxColors.material.SetFloat("_Exposure", alpha / 50000);
            var CloudColor = Clouds.main.startColor;

            if (alpha / 200000 >= 0.15f && alpha / 200000 <= 1)
            {
                CloudColor = new Color((alpha / 150000), (alpha / 150000), (alpha / 150000));

                //SkyboxColors.material.SetColor("_GroundColor", new Color(0, alpha / 200000, 0));
                for (int i = 0; i < DirLight.Length; i++)
                    DirLight[i].intensity = (alpha / 100000);
            }
            if (alpha / 200000 > 0.15f)
            {
                RenderSettings.ambientLight = new Color((alpha / 200000), (alpha / 200000), (alpha / 200000));
                RenderSettings.fogColor = new Color((alpha / 200000) * 0.7f, (alpha / 200000) * 0.6f, (alpha / 200000) * 0.39f);
                //SkyboxColors.material.SetColor("_GroundColor", new Color((alpha / 400000), (alpha / 1000000), 0));
                SkyboxColors.material.SetColor("_GroundColor", new Color((alpha / 200000) * 0.361f, (alpha / 200000) * 0.294f, (alpha / 200000) * 0.180f));
            }
        }
    }

    void ChangeWeather()
    {
        var CloudsEmission = Clouds.emission;
        CloudsEmission.enabled = true;

        CloudsEmission.SetBursts(
            new ParticleSystem.Burst[]{
                new ParticleSystem.Burst(0f, (short)Random.Range(20, 40)),
                new ParticleSystem.Burst(10f, (short)Random.Range(25, 50))
            });
    }

    void MoveCamera()
    {

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (Vector3.Distance(target.transform.position, this.transform.position) <= 15)
            {
                distance -= 1;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (Vector3.Distance(target.transform.position, this.transform.position) <= 15)
            {
                distance += 1;
            }
        }

        if (Vector3.Distance(target.transform.position, this.transform.position) > 15
            && Vector3.Dot(transform.TransformDirection(Vector3.forward), target.position - transform.position) < 0 )
            //characterMovement.TimeElapsedTeleport - 0.05f < Time.time)
        {
            distance += 0.05f;
        }

        if (Vector3.Distance(target.transform.position, this.transform.position) > 15
            && Vector3.Dot(transform.TransformDirection(Vector3.forward), target.position - transform.position) > 0  )
            //characterMovement.TimeElapsedTeleport - 0.05f < Time.time)
        {
            distance -= 0.05f;
        }
    }

    void Start()
    {

        InvokeRepeating("ChangeSkyBoxes", 0, 3);
        InvokeRepeating("ChangeWeather", 0, 120);
        InvokeRepeating("DayAndNight", 0, 0.05f); //0.05 * 20 = 1 second 

        if (photonView.isMine)
        {
            PlayerHeight = 0;
            tempAlpha = 50;
            alpha = 100000; // testing
            characterMovement = transform.GetComponentInParent<CharacterMovement>();
            target = transform.root.transform;
            terrain = GameObject.FindGameObjectWithTag("MainEnvironment").GetComponent<TerrainScript>();
            maingui = terrain.canvas.GetComponent<MainGUI>();
            MapCamera = transform.root.Find("Camera").GetComponent<Camera>();
            SkyBoxCamera = transform.Find("SkyboxCamera").GetComponent<Camera>();
            SkyboxColors = SkyBoxCamera.GetComponentInChildren<Skybox>();
            Sun = terrain.transform.Find("Sun").gameObject;
            Clouds = terrain.transform.Find("Clouds").GetComponent<ParticleSystem>();
            DirLight = terrain.transform.GetComponentsInChildren<Light>();
            RenderSettings.skybox = SkyboxColors.material;
            transform.SetParent(null);

            Vector3 Angles = transform.eulerAngles;
            x = Angles.x;
            y = Angles.y;
            currentDistance = distance;
            desireDistance = distance;
            correctedDistance = distance;
            CameraTarget = target;
            transform.position = target.position;
        }

    }

    void DayAndNight()
    {
        for (int i = 0; i < DirLight.Length; i++)
        {
            DirLight[i].transform.rotation = Quaternion.LookRotation(DirLight[i].transform.position - Sun.transform.position);
        }
        //15 * 20 * 60seconds * 12 minutes ~~ 200000 
        if (alpha <= -200000) // towards day
            tempAlpha = 100;
        else if (alpha > 100000 && alpha < 200000 && tempAlpha == 100)
            tempAlpha = 50;
        else if (alpha >= 200000) // towards night
            tempAlpha = -50;
        else if (alpha > 100000 && alpha < 200000 && tempAlpha == -50)
            tempAlpha = -20;

        //if (alpha <= 200000 && alpha >= -200000) //200000 == most sunlight mid-day, -200000 == middle of night 
            //alpha += tempAlpha;
    }

    private void LateUpdate()
    {
        if (alpha <= 0)
        {
            Sun.GetComponent<MeshRenderer>().enabled = false;
            x = (10000 * Mathf.Cos(Mathf.Abs(alpha) * 0.00001f));
            y = (6500 * Mathf.Sin(Mathf.Abs(alpha) * 0.00001f));
        }
        else
        {
            Sun.GetComponent<MeshRenderer>().enabled = true;
            x = (10000 * Mathf.Cos(alpha * 0.00001f));
            y = (6500 * Mathf.Sin(alpha * 0.00001f));
        }

        Sun.transform.position = new Vector3(x + 4000, y, 4500);

    }

    float yVelocity = 0.0f;
    private float timeCount = 50;
    Quaternion targetRotation;
    Vector3 currAngle;
    public Transform CameraTarget;
    private float mx = 0.0f;
    private float my = 0.0f;

    private int mouseXSpeedMod = 1;
    private int mouseYSpeedMod = 1;

    public float MaxViewDistance = 15f;
    public float MinViewDistance = 1f;
    public int ZoomRate = 45;
    private int lerpRate = 5;
    private float mdistance = 3f;
    private float desireDistance;
    private float correctedDistance;
    private float currentDistance;

    public float cameraTargetHeight = 1.0f;

    //checks if first person mode is on
    private bool click = false;
    //stores cameras distance from player
    private float curDist = 0;

    private float m_refPos = 0;
    private float refPos = 0;

    float targetRotantionAngle;
    float cameraRotationAngle;
    Quaternion rotationX;
    Quaternion rotationXY;

    void Update()
    {


        //CameraTarget.rotation = rotation;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }

    public static float Damp(float source, float target, float smoothing, float dt)
    {
        return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    private void FixedUpdate()
    {
        //transform.position = Vector3.Slerp(transform.position, new Vector3(transform.position.x, terrain.terrain.SampleHeight(transform.position) + 300 + PlayerHeight + 2, transform.position.z), 7 * Time.deltaTime);

        // for running animations, make the hip rock back and forth.
        if (photonView.isMine)
        {
            //if (maingui.DisableAttackWhenMenuItemPressed == false)
            //{

            //    MoveCamera();
            //}

    //        if (Input.GetMouseButton(1))
    //        {
    //            CameraUpandDownRotation -= Input.GetAxis("Mouse Y") * terrain.canvas.GetComponent<MainGUI>().Sensitivity / 2;
    //            CameraUpandDownRotation = Mathf.Clamp(CameraUpandDownRotation, -45, 45);

    //        }

    //        PlayersRotationAngleY = target.eulerAngles.y;
    //        CameraRotationAngleY = transform.eulerAngles.y;

    //        if (characterMovement.state == CharacterMovement.RagdollState.ragdolled ||
    //characterMovement.state == CharacterMovement.RagdollState.blendToAnim)
    //        {
    //            CameraRotationAngleY = Mathf.LerpAngle(CameraRotationAngleY, PlayersRotationAngleY, rotationDamping * 0.25f * Time.smoothDeltaTime); // lerp camera rotation to player rotation
    //            CameraUpandDownRotationSlerped = Mathf.LerpAngle(CameraUpandDownRotation, CameraUpandDownRotationSlerped, 0.25f * Time.smoothDeltaTime);
    //            CameraRotation = Quaternion.Euler(CameraUpandDownRotationSlerped, CameraRotationAngleY, 0); // combine both x and y rotations
    //        }
    //        else if (Time.time <= characterMovement.ragdollingEndTime + 2)
    //        {
    //            CameraRotationAngleY = Mathf.LerpAngle(CameraRotationAngleY, PlayersRotationAngleY, rotationDamping * 0.25f * Time.smoothDeltaTime); // lerp camera rotation to player rotation
    //            CameraUpandDownRotationSlerped = Mathf.LerpAngle(CameraUpandDownRotation, CameraUpandDownRotationSlerped, 0.25f * Time.smoothDeltaTime);
    //            CameraRotation = Quaternion.Euler(CameraUpandDownRotation, PlayersRotationAngleY, 0); // combine both x and y rotations
    //        }
    //        else
    //        {
    //            m_refPos *= Time.deltaTime;
    //            refPos *= Time.deltaTime;

    //            CameraRotationAngleY = Mathf.SmoothDampAngle(CameraRotationAngleY, PlayersRotationAngleY, ref m_refPos, 0.03f);
    //            CameraUpandDownRotationSlerped = Mathf.SmoothDampAngle(CameraUpandDownRotation, CameraUpandDownRotationSlerped, ref refPos, 0.055f);

    //            //CameraRotationAngleY = Mathf.LerpAngle(CameraRotationAngleY, PlayersRotationAngleY, 35 * (1 - Mathf.Exp(-20 * Time.deltaTime))); // lerp camera rotation to player rotation
    //            //CameraUpandDownRotationSlerped = Mathf.LerpAngle(CameraUpandDownRotation, CameraUpandDownRotationSlerped, 35 * (1 - Mathf.Exp(-20 * Time.deltaTime)));

    //            currAngle = new Vector3(CameraUpandDownRotationSlerped, CameraRotationAngleY, 0); // combine both x and y rotations

    //            CameraRotation = Quaternion.Euler(CameraUpandDownRotationSlerped, CameraRotationAngleY, 0);

    //            //CameraRotation = SmoothDampRotation(transform.rotation, Quaternion.Euler(CameraUpandDownRotation, PlayersRotationAngleY, 0), ref m_refPos, 100);
    //        }

            PlayerHeight = target.position.y - terrain.terrain.SampleHeight(target.position) - 300;
            //targetCameraDistance = target.transform.position - ((CameraRotation * Vector3.forward) * distance);

            /// problem is the transform of main character is in jump state when ragdolled
            /// 
            //for hills
            if (transform.position.y < target.transform.position.y)
                tempHeight = target.transform.position.y - transform.position.y;
            else
                tempHeight = 0;


            if (Input.GetMouseButton(1))
            {/*0 mouse btn izq, 1 mouse btn der*/

                //mx += Input.GetAxis("Mouse X") * 1.5f * maingui.Sensitivity;
                //mx += Input.GetAxis("Mouse X") * 1 * maingui.Sensitivity;
                my -= Input.GetAxis("Mouse Y") * 1.5f * maingui.Sensitivity;
                my = ClampAngle(my, -45, 45);
            }
            else if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
            {
                //targetRotantionAngle = target.eulerAngles.y;
                //cameraRotationAngle = transform.eulerAngles.y;
                //mx = Mathf.LerpAngle(cameraRotationAngle, targetRotantionAngle, 5 * Time.deltaTime);
            }

            targetRotantionAngle = target.eulerAngles.y;
            cameraRotationAngle = transform.eulerAngles.y;
            mx = Mathf.LerpAngle(cameraRotationAngle, targetRotantionAngle, 5 * Time.deltaTime);

            //refPos += Time.deltaTime;

            rotationX = Quaternion.Euler(0, targetRotantionAngle, 0);
            rotationXY =  Quaternion.Euler(my, targetRotantionAngle, 0);

            desireDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.fixedDeltaTime * ZoomRate * Mathf.Abs(desireDistance);
            desireDistance = Mathf.Clamp(desireDistance, MinViewDistance, MaxViewDistance);
            correctedDistance = desireDistance;

            Vector3 position = CameraTarget.position - (rotationXY * Vector3.forward * desireDistance);

            //?
            //condicion ? first_expresion : second_expresion;
            //(input > 0) ? isPositive : isNegative;
            position = CameraTarget.position - (rotationXY * Vector3.forward * desireDistance);

            //transform.position = new Vector3(position.x,terrain.terrain.SampleHeight(transform.position) + 300 + PlayerHeight + 3 + tempHeight,position.z);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotationXY, 4 * Time.fixedDeltaTime);
            //transform.position = Vector3.Slerp(transform.position, new Vector3(position.x, terrain.terrain.SampleHeight(transform.position) + 300 + PlayerHeight + 3 + tempHeight,
            //            position.z), 3 * Time.fixedDeltaTime);


            if (characterMovement.state == CharacterMovement.RagdollState.ragdolled ||
                characterMovement.state == CharacterMovement.RagdollState.blendToAnim || Time.time <= characterMovement.ragdollingEndTime + 2)
            {
                distance = 10;
                if (characterMovement.state == CharacterMovement.RagdollState.ragdolled)
                    position = characterMovement.bodyParts[0].transform.position - (transform.rotation * Vector3.forward * distance); // rpbolem here
                else
                    position = target.transform.position - (rotationXY * Vector3.forward * distance);

                transform.position = Vector3.Slerp(transform.position, new Vector3(position.x, terrain.terrain.SampleHeight(transform.position) + 300 + PlayerHeight + 2.5f + tempHeight,
position.z), 4 * Time.deltaTime);

            }
            else if (characterMovement.TimeElapsedTeleport - 0.05f < Time.time)
            {
                transform.position = Vector3.Slerp(transform.position, new Vector3(position.x, terrain.terrain.SampleHeight(transform.position) + 300 + PlayerHeight + 2.5f + tempHeight,
position.z), 4 * Time.deltaTime);
            }
            else
            {
                if (characterMovement.SidetoSideRotation == 0)
                    IncreasingTime += 0.065f;
                else
                    IncreasingTime += 0.1f;

                transform.position = Vector3.Slerp(transform.position, new Vector3(position.x, terrain.terrain.SampleHeight(transform.position) + 300 + PlayerHeight + 2.5f + tempHeight,
position.z), (1 + IncreasingTime) * Time.deltaTime);

            }

            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), 5 * Time.deltaTime); // lerp camera rotation to normal of terrain

            //transform.rotation = CameraRotation;
        }
    }
}
