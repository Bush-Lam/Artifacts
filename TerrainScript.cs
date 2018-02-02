using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TerrainScript : Photon.MonoBehaviour
{
    public Terrain terrain;
    public GameObject Player;
    public Vector3 MasterPosition; // position before going pass bounds
    public GameObject CameraMap;
    public GameObject canvas; // overlay
    public List<GameObject> Players;
    public bool AllComponentsInit;

    public List<GameObject> ListofTrees;
    public GameObject[] TreeGameObjects;

    public List<GameObject> ListofRocks;
    public GameObject[] RockGameObjects;

    public List<GameObject> ListofHerbPatches;
    public GameObject[] HerbPatchObjects;

    public List<GameObject> ListofWaters;

    public List<GameObject> ListofEnvironmentalObjectsAltered; // environmental objects moved by forces
    public List<Vector3> ListofEnvironmentalObjectsAlteredPositions; // initial position of objects
    public List<Quaternion> ListofEnvironmentalObjectsAlteredRotations; // initial rotation of objects

    public GameObject[] Waters;

    public int CurrentTerrainLevel;
    public bool IsThisALoadedGame;
    public AudioSource[] SoundEffects;
    public AudioSource[] Musics;
    public bool[] NotLoading; // finish loading all terrain/monsters

    public int[,] detailLayer1;
    public int[,] detailLayer2;
    public int[,] detailLayer3;
    public int[,] detailLayer4;
    public int[,] detailLayer5;
    public int[,] detailLayer6;
    public int[,] detailLayer7;
    public int[,] detailLayer8;
    public int[,] detailLayer9;
    public int[,] detailLayer10;
    public float[,] terrainMap;
    public float[,,] MiddleAlpha;

    float noise;
    float noise2;
    float noise3 = 0;
    float noise4;
    float noiseEnvironmentX;
    float noiseEnvironmentY;
    public float gainX;
    public float gainXX;
    public float gainY;
    public float gainYY;
    int DetailLayer1Random;
    int DetailLayer2Random;
    int DetailLayer3Random;
    int DetailLayer4Random;
    int DetailLayer5Random;
    int DetailLayer6Random;
    int DetailLayer7Random;

    [PunRPC]
    void AddPlayers(int viewid)
    {
        PhotonView thisPlayer = PhotonView.Find(viewid);
        Players.Add(thisPlayer.gameObject);
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Players.RemoveAt(player.ID - 1);
        PhotonNetwork.RemoveRPCs(player);
        PhotonNetwork.DestroyPlayerObjects(player);
    }

    void OnLevelWasLoaded(int level)
    {
        PhotonNetwork.isMessageQueueRunning = true;
    }

    public Image LoadingScreenImage;
    Image LoadingImage;
    public float ProgressBar;
    int sign = 1;

    IEnumerator LoadScreen() // ienumerator trees,waters,herbs , notloaded[0] should finish loading everything
    {
        //if (NotLoading[0] == false && NotLoading[1] == false) // notloading[1] mainly for multiplayer(wait for all terrain objects to be added to list)
        //{
        //    if (ProgressBar == 0)
        //        sign = 1;
        //    else if (ProgressBar == 100)
        //        sign = -1;
        //    ProgressBar = ProgressBar + sign;

        //    LoadingImage.transform.Find("ProgressBar").GetComponent<Image>().fillAmount = ProgressBar / 100;
        //    yield return new WaitForSeconds(0.02f);
        //    StartCoroutine("LoadScreen");
        //}
        //else
        yield return new WaitForSeconds(0.02f);
        Destroy(LoadingImage.gameObject);
    }

    void Awake()
    {
        // for not starting in main menu - testing
        MainMenu.MultiplayerOnline = true;
        PhotonNetwork.Disconnect();
        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom("Random");
        Players = new List<GameObject>();
        CurrentTerrainLevel = SceneManager.GetActiveScene().buildIndex;

        if (SceneManager.GetActiveScene().name == "Level1")
        {
            Player = PhotonNetwork.Instantiate(Player.name, new Vector3(4500, 600, 4500), transform.rotation, 0)
            as GameObject;
            Players.Add(Player);
            photonView.RPC("AddPlayers", PhotonTargets.OthersBuffered, Player.GetPhotonView().viewID);

            if (Player.GetPhotonView().isMine)
            {
                canvas.GetComponent<CharacterInventoryGUI>().enabled = true;
                Player.GetComponentInChildren<CameraScript>().enabled = true;
                Player.GetComponentInChildren<Camera>().enabled = true;
                Player.GetComponentInChildren<Camera>().transform.Find("SkyboxCamera").GetComponentInChildren<Camera>().enabled = true;
                canvas.GetComponent<MainGUI>().enabled = true;
            }

            //Delete all these, enable the scripts on player manually, then use photonview.ismine where appropiate.

            /*			Player.GetComponent<WeaponsDatabase>().enabled = true;
                        Player.GetComponent<ArmorDatabase>().enabled = true;
                        Player.GetComponent<MiscellaneousItemsDatabase>().enabled = true;
                        Player.GetComponent<BowSkillDatabase>().enabled = true;
                        Player.GetComponent<CraftingSkillDatabase>().enabled = true;
                        Player.GetComponent<GatheringSkillDatabase>().enabled = true;
                        Player.GetComponent<ToolDatabase>().enabled = true;
                        Player.GetComponent<ChopTrees>().enabled = true;
                        Player.GetComponent<PickupObjects>().enabled = true;
                        Player.GetComponent<MineRocks>().enabled = true;
                        Player.GetComponent<Herbloring>().enabled = true;
                        Player.GetComponent<PotionDatabase>().enabled = true;
                        Player.GetComponentInChildren<SaveAndLoadGame>().enabled = true;
            */
            AllComponentsInit = true;
        }
    }

    void Start()
    {
        NotLoading = new bool[2];
        NotLoading[0] = false;
        NotLoading[1] = false;
        LoadingImage = Instantiate(LoadingScreenImage, Vector3.zero, transform.rotation) as Image;
        LoadingImage.transform.SetParent(canvas.transform);
        LoadingImage.transform.localScale = new Vector3(1, 1, 1);
        LoadingImage.transform.localPosition = Vector3.zero;
        StartCoroutine("LoadScreen");

        ListofEnvironmentalObjectsAltered = new List<GameObject>();
        ListofEnvironmentalObjectsAlteredPositions = new List<Vector3>();
        ListofEnvironmentalObjectsAlteredRotations = new List<Quaternion>();

        //problem multiplayer here == too many rpc calls from networkspawntree/rock/etc
        Invoke("AddTrees", 0.5f);
        Invoke("AddRocks", 1);
        Invoke("AddHerbPatches", 1.2f);
        Invoke("AddWaters", 1.4f);

        if (PhotonNetwork.isMasterClient == true)
        {
            if (IsThisALoadedGame == false) // for new game
            {
                gainX = Random.Range(30, 32); // frequency
                gainY = Random.Range(30, 32); // frequency
                gainXX = Random.Range(15, 17); // frequency
                gainYY = Random.Range(15, 17); // frequency

                photonView.RPC("SendTerrainData", PhotonTargets.AllBuffered, gainX, gainY, new Vector3(4500, 0, 4500));
            }
            InvokeRepeating("InvokeNewArea", 0, 1.7f);
        }

        Player.transform.position = new Vector3(Player.transform.position.x, terrain.SampleHeight(Player.transform.position) + 300, Player.transform.position.z);
        Player.GetComponentInChildren<CameraScript>().PlayerHeight = Player.transform.position.y;
    }

    void NewPlayerPositions(int x, int z, int TerrainPosition) // new way to do this = each trees are in range of for ex: 2000meters of each character, then keep their positions, else randomize = do for each environmental object, +2000 east and north, -2000 west and south == for the heightmaps/alphas, everyone spawns on the player that is out of limit and spawns in center.
    {
        for (int i = 0; i < Players.Count; i++)
        {
            switch (TerrainPosition)
            {
                case 1:
                    if (Players[i].transform.position.z > 5000) // north
                    {
                        Players[i].transform.position = new Vector3(Players[i].transform.position.x + x,
                                                                    Players[i].transform.position.y,
                                                                    Players[i].transform.position.z + z);
                    }
                    break;
                case 2:
                    if (Players[i].transform.position.x < 4000) // west
                    {
                        Players[i].transform.position = new Vector3(Players[i].transform.position.x + x,
                                                                    Players[i].transform.position.y,
                                                                    Players[i].transform.position.z + z);
                    }
                    break;
                case 3:
                    if (Players[i].transform.position.x > 5000) //east
                    {
                        Players[i].transform.position = new Vector3(Players[i].transform.position.x + x,
                                                                    Players[i].transform.position.y,
                                                                    Players[i].transform.position.z + z);
                    }
                    break;
                case 4:
                    if (Players[i].transform.position.z < 4000) // south
                    {
                        Players[i].transform.position = new Vector3(Players[i].transform.position.x + x,
                                                                    Players[i].transform.position.y,
                                                                    Players[i].transform.position.z + z);
                    }
                    break;
            }
        }
    }

    [PunRPC]
    void SendTerrainData(float i, float j, Vector3 masterPosition)
    {
        gainX = i;
        gainY = j;
        MasterPosition = masterPosition;
        NewHeightsAndAlphamaps(MasterPosition);
    }

    public void NewHeightsAndAlphamaps(Vector3 TransformPosition)
    {
        //heightmaps are inverted
        int PositionXTransformation = (int)(TransformPosition.x / 9000 * 1024); // x
        int PositionZTransformation = (int)(TransformPosition.z / 9000 * 1024); // y 

        terrainMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        MiddleAlpha = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        detailLayer1 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 0);
        detailLayer2 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 1);
        detailLayer3 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 2);
        detailLayer4 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 3);
        detailLayer5 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 4);
        detailLayer6 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 5);
        detailLayer7 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 6);
        detailLayer8 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 7);
        int numDetails = terrain.terrainData.detailPrototypes.Length;

        //terrain that does not change
        for (int z = 512 - 57; z < 512 + 57; z++) // 57 == 500/9000 * 1024
        {
            for (int x = 512 - 57; x < 512 + 57; x++)
            {
                //get terrain info from original player position then transform that to the center 1024,1024 
                terrainMap[z, x] = 0;
                terrainMap[z, x] = terrainMap[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                MiddleAlpha[z, x, 0] = 0;
                MiddleAlpha[z, x, 1] = 0;
                MiddleAlpha[z, x, 2] = 0;
                MiddleAlpha[z, x, 3] = 0;
                MiddleAlpha[z, x, 0] = MiddleAlpha[z + PositionZTransformation - 512, x + PositionXTransformation - 512, 0];
                MiddleAlpha[z, x, 1] = MiddleAlpha[z + PositionZTransformation - 512, x + PositionXTransformation - 512, 1];
                MiddleAlpha[z, x, 2] = MiddleAlpha[z + PositionZTransformation - 512, x + PositionXTransformation - 512, 2];
                MiddleAlpha[z, x, 3] = MiddleAlpha[z + PositionZTransformation - 512, x + PositionXTransformation - 512, 3];
                detailLayer1[z, x] = 0;
                detailLayer2[z, x] = 0;
                detailLayer3[z, x] = 0;
                detailLayer4[z, x] = 0;
                detailLayer5[z, x] = 0;
                detailLayer6[z, x] = 0;
                detailLayer7[z, x] = 0;
                detailLayer8[z, x] = 0;

                detailLayer1[z, x] = detailLayer1[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                detailLayer2[z, x] = detailLayer2[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                detailLayer3[z, x] = detailLayer3[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                detailLayer4[z, x] = detailLayer4[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                detailLayer5[z, x] = detailLayer5[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                detailLayer6[z, x] = detailLayer6[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                detailLayer7[z, x] = detailLayer7[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
                detailLayer8[z, x] = detailLayer8[z + PositionZTransformation - 512, x + PositionXTransformation - 512];
            }
        }

        terrain.terrainData.SetHeights(0, 0, terrainMap);
        terrain.terrainData.SetAlphamaps(0, 0, MiddleAlpha);
        terrain.terrainData.SetDetailLayer(0, 0, 0, detailLayer1);    // grass   , non-interactive rocks etc
        terrain.terrainData.SetDetailLayer(0, 0, 1, detailLayer2);
        terrain.terrainData.SetDetailLayer(0, 0, 2, detailLayer3);
        terrain.terrainData.SetDetailLayer(0, 0, 3, detailLayer4);
        terrain.terrainData.SetDetailLayer(0, 0, 4, detailLayer5);
        terrain.terrainData.SetDetailLayer(0, 0, 5, detailLayer6);
        terrain.terrainData.SetDetailLayer(0, 0, 6, detailLayer7);
        terrain.terrainData.SetDetailLayer(0, 0, 7, detailLayer8);

        // terrain that changes

        terrainMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight);
        MiddleAlpha = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        detailLayer1 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 0);
        detailLayer2 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 1);
        detailLayer3 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 2);
        detailLayer4 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 3);
        detailLayer5 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 4);
        detailLayer6 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 5);
        detailLayer7 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 6);
        detailLayer8 = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 7);

        //for (int i = 0; i < 1025; i++) // stitch half terrain
        //{
        //    for (int j = 0; j < 1025; j++)
        //    {
        //        if (i == 1024 || j == 1024)
        //            terrainMap[i, j] = -500;

        //        for (int x = 0; x < 10; x++)
        //        {
        //            terrainMap[i, 512 + 57 + x] = (terrainMap[i, 512 + x  + 57 - 1] + terrainMap[i, 512 + x + 57 + 1]) / 2; // top 
        //            terrainMap[512 + x + 57, j] = (terrainMap[512 + x + 57 - 1, j] + terrainMap[512 + x + 57 + 1, j]) / 2; // right

        //            terrainMap[i, 512 - 57 - x] = (terrainMap[i, 512 - 57 - x - 1] + terrainMap[i, 512 - 57 - x + 1]) / 2; // bottom
        //            terrainMap[512 - 57 - x, j] = (terrainMap[512 - 57 - x - 1, j] + terrainMap[512 - 57 - x + 1, j]) / 2; // left
        //        }
        //    }
        //}

        for (int z = 0; z < 1024; z++) // 57 == 500/9000 * 1024
        {
            for (int x = 0; x < 1024; x++)
            {
                if ((z > 512 + 57 && z < 1024 - 57 && x > 512 + 57 && x < 1024 - 57) ||
                    (PositionXTransformation == 512 && PositionZTransformation == 512))
                {

                    terrainMap[z, x] = 0;

                    noise = 0;
                    noise2 = 0;
                    noise3 = 0;

                    // /2048 == amplitude , gainx,gainy * frequency

                    for (int i = 0; i < 4; i++)
                        noise -= Mathf.PerlinNoise((z * gainY * 0.75f) / 1024, (x * gainX * 0.75f) / 1024) * gainXX / 7;
                    for (int i = 0; i < 6; i++)
                        noise2 = Mathf.PerlinNoise((z * gainX * 1) / 3048, (x * gainY * 1) / 3048) * gainYY / 5;
                    for (int i = 0; i < 8; i++)
                        noise3 += Mathf.PerlinNoise((z * gainY * 1) / 2048, (x * gainX * 1) / 2048) * (gainXX + gainYY) / 12;

                    float b = 35;

                    MiddleAlpha[z, x, 0] = 1;
                    MiddleAlpha[z, x, 3] = 0;
                    MiddleAlpha[z, x, 2] = 0;
                    MiddleAlpha[z, x, 1] = 0;

                    MiddleAlpha[z, x, 3] = noise2;

                    terrainMap[z, x] = ((noise + noise2 + noise3) / b) * 0.995f;

                    for (int i = 0; i < 3; i++)
                        noise4 = Mathf.PerlinNoise((z * gainX * 1.1f) / 1024, (x * gainY * 1.1f) / 1024) * 10 / gainX;

                    if (Random.Range(0, 500) == 1)
                        Debug.Log((noise4));

                    if (noise4 < 0.05f) // rocks
                    {
                        terrainMap[z, x] = ((noise + noise2 + noise3) / b) * 0.995f;
                        MiddleAlpha[z, x, 0] = 0;
                        MiddleAlpha[z, x, 1] = 1;
                        MiddleAlpha[z, x, 2] = 0;
                        MiddleAlpha[z, x, 3] = 0;
                    }
                    else if ((noise + noise2 + noise3) / b <= 0) // water == how to do water, random water positions then make that area the brown alpha within a lighter grass area
                    {
                        terrainMap[z, x] = ((noise + noise2 + noise3) / b) * 0.995f;
                        MiddleAlpha[z, x, 0] = 0;
                        MiddleAlpha[z, x, 1] = 0;
                        MiddleAlpha[z, x, 2] = 1;
                        MiddleAlpha[z, x, 3] = 0;

                    }

                    if (MiddleAlpha[z, x, 1] <= 0)
                    {
                        //if (noise2 + noise3 + noise > 1)
                        DetailLayer7Random = (int)((noise2 / 2 + noise / 2 + noise3 / 2) * 13); // orange and green grass

                        DetailLayer1Random = 0;
                        DetailLayer2Random = 0;
                        DetailLayer3Random = 0;
                        DetailLayer4Random = 0;
                        DetailLayer5Random = 0;
                        DetailLayer6Random = 0;

                        // somehow sync this

                        for (int i = 0; i < 3; i++)
                            noise4 = Mathf.PerlinNoise((z * gainX * 6) / 1024, (x * gainY * 4) / 1024) * 1 / (gainXX + gainYY);

                        if (noise4 >= 0.005f)
                            DetailLayer2Random = 2;

                        if (noise4 * 100 < 1 || noise4 * 100 > 5.5f)
                            DetailLayer3Random = (int)(noise4 * 200);
                        else
                            DetailLayer3Random = 0;

                        //if (noise2 + noise3 + noise > 2.5f && noise2 + noise3 + noise < 2.6f)
                        //    DetailLayer5Random = 2;
                        //if (noise2 + noise3 + noise > 2 && noise2 + noise3 + noise < 2.1f)
                        //    DetailLayer6Random = 3;
                        //if (noise2 + noise3 + noise > 1.5f && noise2 + noise3 + noise < 1.6f)
                        //    DetailLayer1Random = 3;

                        if (MiddleAlpha[z, x, 2] <= 0)
                        {
                            detailLayer8[z, x] = DetailLayer7Random; // green grass
                            detailLayer7[z, x] = DetailLayer7Random; // orange grass
                            detailLayer1[z, x] = DetailLayer1Random;
                            detailLayer3[z, x] = DetailLayer3Random;
                            detailLayer4[z, x] = DetailLayer4Random;
                            detailLayer5[z, x] = DetailLayer5Random;
                            detailLayer6[z, x] = DetailLayer6Random;
                        }
                        if (MiddleAlpha[z, x, 2] > 0)
                        {
                            detailLayer7[z, x] = 0;
                            detailLayer1[z, x] = 0;
                            detailLayer2[z, x] = 0;
                            detailLayer3[z, x] = 0;
                            detailLayer4[z, x] = 0;
                            detailLayer5[z, x] = 0;
                            detailLayer6[z, x] = 0;
                            detailLayer8[z, x] = 0; // green grass
                        }
                    }
                    else
                    {
                        detailLayer7[z, x] = 0;
                        detailLayer1[z, x] = 0;
                        detailLayer2[z, x] = 0;
                        detailLayer3[z, x] = 0;
                        detailLayer4[z, x] = 0;
                        detailLayer5[z, x] = 0;
                        detailLayer6[z, x] = 0;
                        detailLayer8[z, x] = 0; // green grass
                    }
                }
                if ((z > 910 || z < 57 || x > 910 || x < 57))
                {
                    MiddleAlpha[z, x, 3] = 0;

                    detailLayer1[z, x] = 0;
                    detailLayer2[z, x] = 0;
                    detailLayer3[z, x] = 0;
                    detailLayer4[z, x] = 0;
                    detailLayer5[z, x] = 0;
                    detailLayer6[z, x] = 0;
                }
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, MiddleAlpha);
        terrain.terrainData.SetHeights(0, 0, terrainMap);
        terrain.terrainData.SetDetailLayer(0, 0, 0, detailLayer1);    // grass   , non-interactive rocks etc
        terrain.terrainData.SetDetailLayer(0, 0, 1, detailLayer2);
        terrain.terrainData.SetDetailLayer(0, 0, 2, detailLayer3);
        terrain.terrainData.SetDetailLayer(0, 0, 3, detailLayer4);
        terrain.terrainData.SetDetailLayer(0, 0, 4, detailLayer5);
        terrain.terrainData.SetDetailLayer(0, 0, 5, detailLayer6);
        terrain.terrainData.SetDetailLayer(0, 0, 6, detailLayer7);
        terrain.terrainData.SetDetailLayer(0, 0, 7, detailLayer8);

        if (PositionXTransformation == 512 && PositionZTransformation == 512 && PhotonNetwork.isMasterClient)
        {
            Invoke("NetworksInstantiateTrees", 2);
        }
    }

    void InvokeNewArea()
    {
        StartCoroutine("EnterNewArea");
    }

    void NewAreaEnvironmentalObjs(GameObject[] EnvObjs, string EnvType, Vector3 PlayerPosition)
    {
        for (int i = 0; i < EnvObjs.Length; i++)
        {
            if (Vector3.Distance(EnvObjs[i].transform.position, PlayerPosition) < 500)
            {
                float x = 4500 + (4500 + (PlayerPosition.x - EnvObjs[i].transform.position.x));
                float z = 4500 + (4500 + (PlayerPosition.z - EnvObjs[i].transform.position.z));

                switch (EnvType) // environmental objs within 500 meters from players
                {
                    case "Tree":
                        RandomTreePositions(MiddleAlpha, i, x, z, 4500 - 500, 4500 + 500, 4500 - 500, 4500 + 500, 1);
                        break;
                    case "Rock":
                        StartCoroutine(RandomTreePositions(MiddleAlpha, i, x, z, 4500 - 500, 4500 + 500, 4500 - 500, 4500 + 500, 1));
                        break;
                    case "Herb":
                        RandomHerbPatchPositions(MiddleAlpha, i, x, z, 4500 - 500, 4500 + 500, 4500 - 500, 4500 + 500, 1);
                        break;
                    case "Water":
                        RandomWaterPositions(MiddleAlpha, i, x, z, 4500 - 500, 4500 + 500, 4500 - 500, 4500 + 500);
                        break;
                }
            }
            else // player spawns at 4500,4500 everything greater than 500m from player randomize
            {
                float x_max = Random.Range(5000, 9000);
                float x_min = Random.Range(0, 4000);

                float z_max = Random.Range(5000, 9000);
                float z_min = Random.Range(0, 4000);

                if (Random.Range(0, 2) == 1)
                {
                    switch (EnvType)
                    {
                        case "Tree":
                            RandomTreePositions(MiddleAlpha, i, x_max, z_max, 5000, 9000, 5000, 9000, 1);
                            break;
                        case "Rock":
                            StartCoroutine(RandomRockPositions(MiddleAlpha, i, x_max, z_max, 5000, 9000, 5000, 9000, 1));
                            break;
                        case "Herb":
                            RandomHerbPatchPositions(MiddleAlpha, i, x_max, z_max, 5000, 9000, 5000, 9000, 1);
                            break;
                        case "Water":
                            RandomWaterPositions(MiddleAlpha, i, x_max, z_max, 5000, 9000, 5000, 9000);
                            break;
                    }
                }
                else
                {
                    switch (EnvType)
                    {
                        case "Tree":
                            RandomTreePositions(MiddleAlpha, i, x_max, z_max, 0, 4000, 0, 4000, 1);
                            break;
                        case "Rock":
                            StartCoroutine(RandomRockPositions(MiddleAlpha, i, x_max, z_max, 0, 4000, 0, 4000, 1));
                            break;
                        case "Herb":
                            RandomHerbPatchPositions(MiddleAlpha, i, x_max, z_max, 0, 4000, 0, 4000, 1);
                            break;
                        case "Water":
                            RandomWaterPositions(MiddleAlpha, i, x_max, z_max, 0, 4000, 0, 4000);
                            break;
                    }
                }
            }
        }
    }

    IEnumerator EnterNewArea()
    {
        //height map width and height = 1024, 

        // 1000 / 9000 =~ 0.1111, 0.1111 * 1024 =~ 57 , 8000/9000* 1024 = 910       
        for (int a = 0; a < Players.Count; a++)
        {
            if (PhotonNetwork.isMasterClient) // if too far from master player respawn
            {
                if (Vector3.Distance(Players[a].transform.position, Players[0].transform.position) > 250)
                    Players[a].transform.position = Players[0].transform.position;
            }

            if (Players[a].transform.position.x > 8500 ||
                Players[a].transform.position.x < 500 ||
                Players[a].transform.position.z > 8500 ||
                Players[a].transform.position.z < 500)
            {
                Vector3 OrigPlayerPosition = Players[a].transform.position;

                if (PhotonNetwork.isMasterClient == true)
                {
                    gainX = Random.Range(28, 35); // frequency
                    gainY = Random.Range(28, 35); // frequency

                    photonView.RPC("SendTerrainData", PhotonTargets.AllBufferedViaServer, gainX, gainY, OrigPlayerPosition);
                }
                //offset
                float xOffset = 0;
                float zOffset = 0;
                if (OrigPlayerPosition.x < 500)
                {
                    NewHeightsAndAlphamaps(new Vector3(502, Players[a].transform.position.y, OrigPlayerPosition.z));
                    xOffset = Players[a].transform.position.x - 502;
                }
                else if (OrigPlayerPosition.x > 8500)
                {
                    NewHeightsAndAlphamaps(new Vector3(8500, Players[a].transform.position.y, OrigPlayerPosition.z));
                    xOffset = Players[a].transform.position.x - 8500;
                }
                else if (OrigPlayerPosition.z < 500)
                {
                    NewHeightsAndAlphamaps(new Vector3(OrigPlayerPosition.x, Players[a].transform.position.y, 502));
                    zOffset = Players[a].transform.position.z - 502;
                }
                else if (OrigPlayerPosition.z > 8500)
                {
                    NewHeightsAndAlphamaps(new Vector3(OrigPlayerPosition.x, Players[a].transform.position.y, 8500));
                    zOffset = Players[a].transform.position.z - 8500;
                }

                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].transform.position = new Vector3(4500 + xOffset + Players[i].transform.position.x - Players[a].transform.position.x, 0, 4500 + Players[i].transform.position.z + zOffset - Players[a].transform.position.z);
                }

                yield return new WaitForSeconds(1);

                NewAreaEnvironmentalObjs(ListofTrees.ToArray(), "Tree", Players[a].transform.position);
                NewAreaEnvironmentalObjs(ListofRocks.ToArray(), "Rock", Players[a].transform.position);
                NewAreaEnvironmentalObjs(ListofHerbPatches.ToArray(), "Herb", Players[a].transform.position);
                NewAreaEnvironmentalObjs(ListofWaters.ToArray(), "Water", Players[a].transform.position);
            }
        }
    }

    [PunRPC]
    void NetworkRocks(int i, Vector3 pos, Quaternion rot, float RandomScale)
    {
        ListofRocks[i].transform.position = pos;
        ListofRocks[i].transform.rotation = rot;
        ListofRocks[i].transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);
        ListofRocks[i].GetComponent<PhotonTransformView>().enabled = false;
        ListofRocks[i].GetComponent<PhotonRigidbodyView>().enabled = false;
        ListofRocks[i].GetComponent<PhotonView>().photonView.synchronization = ViewSynchronization.Off;
        ListofRocks[i].transform.GetComponent<Rigidbody>().mass = 3000 * RandomScale;
    }

    [PunRPC]
    void NetworkTrees(Vector3[] pos, Quaternion[] rot, Vector3[] RandomScale)
    {
        for (int i = 0; i < ListofTrees.Count; i++)
        {
            ListofTrees[i].transform.position = pos[i];
            ListofTrees[i].transform.rotation = rot[i];
            ListofTrees[i].transform.localScale = RandomScale[i];
            ListofTrees[i].GetComponent<PhotonTransformView>().enabled = false;
            ListofTrees[i].GetComponent<PhotonRigidbodyView>().enabled = false;
            ListofTrees[i].GetComponent<PhotonView>().photonView.synchronization = ViewSynchronization.Off;
            ListofTrees[i].transform.GetComponent<Rigidbody>().mass = 3000 * RandomScale[i].x;

        }
    }

    [PunRPC]
    void NetworkWater(int i, Vector3 pos, Quaternion rot, float RandomScale)
    {
        ListofWaters[i].transform.position = pos;
        ListofWaters[i].transform.rotation = rot;
        ListofWaters[i].transform.localScale = new Vector3(RandomScale, RandomScale, 2);
    }

    [PunRPC]
    void NetworkHerbPatches(int i, Vector3 pos, Quaternion rot, float[] x, float[] y)
    {
        ListofHerbPatches[i].transform.position = pos;
        ListofHerbPatches[i].transform.rotation = rot;

        Transform[] positions = ListofHerbPatches[i].GetComponentsInChildren<Transform>();

        for (int j = 1; j < positions.Length; j++) // positions of individual herbs 
        {
            positions[j].transform.localPosition = new Vector3(x[j - 1], 0, y[j - 1]); // fix

            positions[j].transform.position = new Vector3(ListofHerbPatches[i].transform.position.x + x[j - 1], terrain.SampleHeight(positions[j].transform.position) +
                                                                terrain.transform.position.y, ListofHerbPatches[i].transform.position.z + y[j - 1]);
        }

        if (i >= ListofHerbPatches.Count - 3)
        {
            NotLoading[0] = true;
            NotLoading[1] = true;
        }
    }

    void AddTrees()
    {
        Rigidbody[] Trees = GameObject.Find("TreesList").GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < Trees.Length; i++)
            ListofTrees.Add(Trees[i].gameObject);
    }

    void AddRocks()
    {
        Transform[] Rocks = GameObject.Find("RocksList").GetComponentsInChildren<Transform>();
        for (int i = 0; i < Rocks.Length; i++)
        {
            ListofRocks.Add(Rocks[i].gameObject);
        }
    }

    void AddHerbPatches()
    {
        Transform[] Herbs = GameObject.Find("HerbsList").GetComponentsInChildren<Transform>();

        for (int i = 0; i < Herbs.Length; i++)
        {
            ListofHerbPatches.Add(Herbs[i].gameObject);
        }
    }

    void AddWaters()
    {
        Transform[] waters = GameObject.Find("WaterList").GetComponentsInChildren<Transform>();

        for (int i = 0; i < waters.Length; i++)
        {
            ListofWaters.Add(waters[i].gameObject);
            if (ListofWaters[i].name != "WaterList")
                ListofWaters[i].GetComponent<MeshRenderer>().sharedMaterial.shader.maximumLOD = 201;
        }
    }

    IEnumerator RandomTreePositions(float[,,] MiddleAlpha, int TreeIndex, float TreePositionX, float TreePositionZ, int MinX, int MaxX, int MinZ, int MaxZ, int SpawnTreeIndex)
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (MiddleAlpha[(int)((TreePositionZ / 9000) * 1024), (int)((TreePositionX / 9000) * 1024), 1] == 0)
            {
                for (int j = 0; j < SpawnTreeIndex; j++) // for grouping similar rocks together(4) or 1 if they are outside of the black
                {
                    float signX = 0;
                    float signZ = 0;
                    float RandomScale = Random.Range(2, 4);

                    ListofTrees[TreeIndex + j].transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);

                    if (j == 1)
                    {
                        signX = Random.Range(8, 65) * RandomScale;
                        signZ = -Random.Range(8, 65) * RandomScale;
                    }
                    else if (j == 2)
                    {
                        signX = Random.Range(8, 65) * RandomScale;
                        signZ = Random.Range(8, 65) * RandomScale;
                    }
                    else if (j == 3)
                    {
                        signX = -Random.Range(8, 65) * RandomScale;
                        signZ = -Random.Range(8, 65) * RandomScale;
                    }

                    ListofTrees[TreeIndex + j].transform.GetComponent<Rigidbody>().mass = 3000 * RandomScale;

                    ListofTrees[TreeIndex + j].transform.position = new Vector3(TreePositionX + signX, 0
                                                            , TreePositionZ + signZ);
                    ListofTrees[TreeIndex + j].transform.position = new Vector3(TreePositionX + signX, terrain.SampleHeight(ListofTrees[TreeIndex + j].transform.position) +
                                                                            terrain.transform.position.y - RandomScale*3, TreePositionZ + signZ); // pivot point for the trees?

                    // synchronize for a split second then turn synchronize off to sync the enviornmental objs.

                    ListofTrees[TreeIndex + j].GetComponent<PhotonTransformView>().enabled = false;
                    ListofTrees[TreeIndex + j].GetComponent<PhotonRigidbodyView>().enabled = false;
                    ListofTrees[TreeIndex + j].GetComponent<PhotonView>().photonView.synchronization = ViewSynchronization.Off;
                    yield return new WaitForSeconds(0.005f);

                    if (TreeIndex + j >= ListofTrees.Count - 1)
                        Invoke("NetworksInstantiateRocks", 1);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.05f);
                TreePositionX = Random.Range(MinX, MaxX);
                TreePositionZ = Random.Range(MinZ, MaxZ);
                StartCoroutine(RandomTreePositions(MiddleAlpha, TreeIndex, TreePositionX, TreePositionZ, MinX, MaxX, MinZ, MaxZ, SpawnTreeIndex));
            }
        }

    }

    IEnumerator RandomRockPositions(float[,,] MiddleAlpha, int RockIndex, float RockPositionX, float RockPositionZ, int MinX, int MaxX, int MinZ, int MaxZ, int SpawnRockIndex)
    {
        if (MiddleAlpha[(int)((RockPositionZ / 9000) * 1024), (int)((RockPositionX / 9000) * 1024), 1] > 0)
        {
            for (int j = 0; j < SpawnRockIndex; j++)
            {
                float signX = 0;
                float signZ = 0;
                float RandomScale = Random.Range(1, 1.66f);
                ListofRocks[RockIndex + j].transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);

                if (j == 0)
                {
                    signX = 6 * RandomScale;
                    signZ = -6 * RandomScale;
                }
                else if (j == 1)
                {
                    signX = 6 * RandomScale;
                    signZ = 6 * RandomScale;
                }
                else if (j == 2)
                {
                    signX = -6 * RandomScale;
                    signZ = 6 * RandomScale;
                }
                else if (j == 3)
                {
                    signX = -6 * RandomScale;
                    signZ = -6 * RandomScale;
                }
                else if (j == 4)
                {
                    signX = 9 * RandomScale;
                    signZ = -9 * RandomScale;
                }
                else if (j == 5)
                {
                    signX = -9 * RandomScale;
                    signZ = 9 * RandomScale;
                }

                ListofRocks[RockIndex + j].transform.position = new Vector3(RockPositionX + signX, 0
                                                                    , RockPositionZ + signZ);
                ListofRocks[RockIndex + j].transform.rotation = Quaternion.identity;

                Vector3 currentup = Vector3.up;
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(RockPositionX + signX, terrain.SampleHeight(ListofRocks[RockIndex + j].transform.position) +
                                                            terrain.transform.position.y + 5, RockPositionZ + signZ), -currentup, out hit, 60))
                {
                    currentup = hit.normal;

                    ListofRocks[RockIndex + j].GetComponent<PhotonTransformView>().enabled = false;
                    ListofRocks[RockIndex + j].GetComponent<PhotonRigidbodyView>().enabled = false;
                    ListofRocks[RockIndex + j].GetComponent<PhotonView>().photonView.synchronization = ViewSynchronization.Off;
                    ListofRocks[RockIndex + j].transform.rotation = Quaternion.FromToRotation(ListofRocks[RockIndex + j].transform.up, hit.normal) * Quaternion.Euler(-90, 0, 0);
                }

                ListofRocks[RockIndex + j].transform.GetComponent<Rigidbody>().mass = 1500 * RandomScale;
                ListofRocks[RockIndex + j].transform.position = new Vector3(RockPositionX + signX, terrain.SampleHeight(ListofRocks[RockIndex + j].transform.position) +
                                                            terrain.transform.position.y - 0.3f, RockPositionZ + signZ);

                if (ListofRocks[RockIndex + j].transform.position.z < 5 || ListofRocks[RockIndex + j].transform.position.z > 8995 ||
                    ListofRocks[RockIndex + j].transform.position.x < 5 || ListofRocks[RockIndex + j].transform.position.x > 8995) // out of map
                {
                    yield return new WaitForSeconds(0.02f);
                    RockPositionX = Random.Range(MinX, MaxX);
                    RockPositionZ = Random.Range(MinZ, MaxZ);
                    StartCoroutine(RandomRockPositions(MiddleAlpha, RockIndex, RockPositionX, RockPositionZ, MinX, MaxX, MinZ, MaxZ, 3));
                    yield break;
                }

                if (RockIndex + j >= ListofRocks.Count - 1)
                {
                    Invoke("NetworksInstantiateWaters", 1);
                }

                if ((MiddleAlpha[(int)((ListofRocks[RockIndex + j].transform.position.z / 9000) * 1024), (int)((ListofRocks[RockIndex + j].transform.position.x / 9000) * 1024), 1] == 0))
                {
                    yield return new WaitForSeconds(0.02f);
                    RockPositionX = Random.Range(MinX, MaxX);
                    RockPositionZ = Random.Range(MinZ, MaxZ);
                    StartCoroutine(RandomRockPositions(MiddleAlpha, RockIndex, RockPositionX, RockPositionZ, MinX, MaxX, MinZ, MaxZ, 3));
                    yield break;
                } // problem here

                Collider[] Objs = Physics.OverlapSphere(ListofRocks[RockIndex].transform.position, RandomScale * 5);

                for (int i = 0; i < Objs.Length; i++)
                {
                    if ((Objs[i].tag == "Environment" && Objs[i].gameObject != ListofRocks[RockIndex].gameObject))
                    {
                        yield return new WaitForSeconds(0.02f);
                        RockPositionX = Random.Range(MinX, MaxX);
                        RockPositionZ = Random.Range(MinZ, MaxZ);
                        StartCoroutine(RandomRockPositions(MiddleAlpha, RockIndex, RockPositionX, RockPositionZ, MinX, MaxX, MinZ, MaxZ, 3));
                        yield break;
                    }
                }

                //photonView.RPC("NetworkRocks", PhotonTargets.OthersBuffered, RockIndex, ListofRocks[RockIndex].transform.position, ListofRocks[RockIndex].transform.rotation, RandomScale);
                yield return new WaitForSeconds(0.05f);
            }
        }
        else
        {
            RockPositionX = Random.Range(MinX, MaxX);
            RockPositionZ = Random.Range(MinZ, MaxZ);

            yield return new WaitForSeconds(0.02f);
            StartCoroutine(RandomRockPositions(MiddleAlpha, RockIndex, RockPositionX, RockPositionZ, MinX, MaxX, MinZ, MaxZ, SpawnRockIndex));
        }
    }

    IEnumerator RandomHerbPatchPositions(float[,,] MiddleAlpha, int HerbPatchIndex, float HerbPatchPositionX, float HerbPatchPositionZ, int MinX, int MaxX, int MinZ, int MaxZ, int SpawnPatchIndex)
    {

        if (MiddleAlpha[(int)((HerbPatchPositionZ / 9000) * 1024), (int)((HerbPatchPositionX / 9000) * 1024), 0] == 1)
        {
            for (int j = 0; j < SpawnPatchIndex; j++) // for grouping similar rocks together(4) or 1 if they are outside of the black
            {
                if (HerbPatchIndex + j >= ListofHerbPatches.Count - 1)             //here == put all networkspawn terrain objects here == when the last herb is spawned;
                {
                    NotLoading[0] = true;
                    NotLoading[1] = true;

                    Vector3[] ListofTreesPosition = new Vector3[ListofTrees.Count];
                    Quaternion[] ListofTreesRotation = new Quaternion[ListofTrees.Count];
                    Vector3[] ListofTreesScale = new Vector3[ListofTrees.Count];

                    for (int i = 0; i < ListofTrees.Count; i++)
                    {
                        ListofTreesPosition[i] = ListofTrees[i].transform.position;
                        ListofTreesRotation[i] = ListofTrees[i].transform.rotation;
                        ListofTreesScale[i] = ListofTrees[i].transform.localScale;
                    }

                    for (int x = 0; x < terrain.GetComponent<MonsterSpawn>().ListofMonsters.Count; x++)
                    {
                        terrain.GetComponent<MonsterSpawn>().ListofMonsters[x].SetActive(true);
                    }

                    //photonView.RPC("NetworkTrees", PhotonTargets.OthersBuffered, (Vector3[])ListofTreesPosition, (Quaternion[])ListofTreesRotation, (Vector3[])ListofTreesScale);
                }

                float signX = 0;
                float signZ = 0;
                float RandomScale = Random.Range(1.3f, 1.66f);

                ListofHerbPatches[HerbPatchIndex + j].transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale);

                if (j == 1)
                {
                    signX = Random.Range(4, 8) * RandomScale;
                    signZ = -Random.Range(4, 8) * RandomScale;
                }
                else if (j == 2)
                {
                    signX = Random.Range(4, 8) * RandomScale;
                    signZ = Random.Range(4, 8) * RandomScale;
                }
                else if (j == 3)
                {
                    signX = -Random.Range(4, 8) * RandomScale;
                    signZ = -Random.Range(4, 8) * RandomScale;
                }

                ListofHerbPatches[HerbPatchIndex + j].transform.position = new Vector3(HerbPatchPositionX + signX, 0
                                        , HerbPatchPositionZ + signZ);
                ListofHerbPatches[HerbPatchIndex + j].transform.rotation = Quaternion.identity;

                Vector3 currentup = Vector3.up;
                RaycastHit hit;

                if (Physics.Raycast(new Vector3(HerbPatchPositionX + signX, terrain.SampleHeight(ListofHerbPatches[HerbPatchIndex + j].transform.position) +
                                                            terrain.transform.position.y + 5, HerbPatchPositionZ + signZ), -currentup, out hit, 60))
                {
                    currentup = hit.normal;
                    ListofHerbPatches[HerbPatchIndex + j].transform.rotation = Quaternion.FromToRotation(ListofHerbPatches[HerbPatchIndex + j].transform.up, hit.normal) * Quaternion.Euler(-90, 0, 0);
                }

                ListofHerbPatches[HerbPatchIndex + j].transform.position = new Vector3(HerbPatchPositionX + signX, terrain.SampleHeight(ListofHerbPatches[HerbPatchIndex + j].transform.position) +
                                                                        terrain.transform.position.y, HerbPatchPositionZ + signZ);

                ListofHerbPatches[HerbPatchIndex + j].GetComponent<PhotonView>().photonView.synchronization = ViewSynchronization.Off;

                yield return new WaitForSeconds(0.05f);
            }


            //photonView.RPC("NetworkHerbPatches", PhotonTargets.OthersBuffered, HerbPatchIndex, ListofHerbPatches[HerbPatchIndex].transform.position, ListofHerbPatches[HerbPatchIndex].transform.rotation, x, y);
        }
        else
        {
            HerbPatchPositionX = Random.Range(MinX, MaxX);
            HerbPatchPositionZ = Random.Range(MinZ, MaxZ);

            yield return new WaitForSeconds(0.02f);
            StartCoroutine(RandomHerbPatchPositions(MiddleAlpha, HerbPatchIndex, HerbPatchPositionX, HerbPatchPositionZ, MinX, MaxX, MinZ, MaxZ, SpawnPatchIndex));
        }

    }

    IEnumerator RandomWaterPositions(float[,,] MiddleAlpha, int WaterIndex, float WaterPositionX, float WaterPositionZ, int MinX, int MaxX, int MinZ, int MaxZ)
    {
        if (MiddleAlpha[(int)((WaterPositionZ / 9000) * 1024), (int)((WaterPositionX / 9000) * 1024), 2] > 0.8f &&
            MiddleAlpha[(int)((WaterPositionZ / 9000) * 1024), (int)((WaterPositionX / 9000) * 1024), 0] == 0)
        {
            ListofWaters[WaterIndex].transform.position = new Vector3(WaterPositionX, 0
                                                                        , WaterPositionZ);
            ListofWaters[WaterIndex].transform.rotation = Quaternion.identity;
            float RandomScale = Random.Range(40, 45);
            ListofWaters[WaterIndex].transform.localScale = new Vector3(RandomScale, RandomScale, RandomScale / 8);

            //Vector3 currentup = Vector3.up;
            //RaycastHit hit;        

            ListofWaters[WaterIndex].transform.rotation = Quaternion.Euler(-90, 0, 0);
            ListofWaters[WaterIndex].transform.position = new Vector3(WaterPositionX, terrain.SampleHeight(ListofWaters[WaterIndex].transform.position) +
                                                                    terrain.transform.position.y, WaterPositionZ);

            Collider[] Objs = Physics.OverlapSphere(ListofWaters[WaterIndex].transform.position, RandomScale * 2);

            for (int i = 0; i < Objs.Length; i++)
            {
                if (((Objs[i].name == "Water2(Clone)" || Objs[i].name == "Water3(Clone)")) && Objs[i].gameObject != ListofWaters[WaterIndex].gameObject)
                {
                    yield return new WaitForSeconds(0.02f);
                    WaterPositionX = Random.Range(MinX, MaxX);
                    WaterPositionZ = Random.Range(MinZ, MaxZ);
                    StartCoroutine(RandomWaterPositions(MiddleAlpha, WaterIndex, WaterPositionX, WaterPositionZ, MinX, MaxX, MinZ, MaxZ));
                    yield break;
                }
            }

            if (WaterIndex >= ListofWaters.Count - 2)
            {
                Invoke("NetworksInstantiateHerbPatches", 1);
            }

            //photonView.RPC("NetworkWater", PhotonTargets.OthersBuffered, WaterIndex, ListofWaters[WaterIndex].transform.position, ListofWaters[WaterIndex].transform.rotation, RandomScale);
        }
        else
        {
            WaterPositionX = Random.Range(MinX, MaxX);
            WaterPositionZ = Random.Range(MinZ, MaxZ);

            yield return new WaitForSeconds(0.05f);
            StartCoroutine(RandomWaterPositions(MiddleAlpha, WaterIndex, WaterPositionX, WaterPositionZ, MinX, MaxX, MinZ, MaxZ));
        }

    }

    void NetworksInstantiateTrees()
    {

        Rigidbody[] TreesLength = GameObject.Find("TreesList").GetComponentsInChildren<Rigidbody>();

        float[] TreePositionX = new float[TreesLength.Length]; //4800
        float[] TreePositionZ = new float[TreesLength.Length];
        int i = 1;

        float[,,] MiddleAlpha = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

        if (IsThisALoadedGame == false)
        {
            for (i = 0; i < TreesLength.Length; i += 4) // start at 0 since treeslist doesnt have a rigidbody
            {
                TreePositionX[i] = Random.Range(0, 9000);
                TreePositionZ[i] = Random.Range(0, 9000);
                StartCoroutine(RandomTreePositions(MiddleAlpha, i, TreePositionX[i], TreePositionZ[i], 0, 9000, 0, 9000, 4));
            }
        }
    }

    void NetworksInstantiateRocks()
    {

        Transform[] RocksLength = GameObject.Find("RocksList").GetComponentsInChildren<Transform>();

        float[] RockPositionX = new float[RocksLength.Length];
        float[] RockPositionZ = new float[RocksLength.Length];

        float[,,] MiddleAlpha = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

        if (IsThisALoadedGame == false)
        {
            for (int i = 1; i < RocksLength.Length; i += 3) // start at 1 since rockslist has a transform
            {
                RockPositionX[i] = Random.Range(0, 9000);
                RockPositionZ[i] = Random.Range(0, 9000);

                StartCoroutine(RandomRockPositions(MiddleAlpha, i, RockPositionX[i], RockPositionZ[i], 0, 9000, 0, 9000, 3));
            }
        }
    }

    void NetworksInstantiateHerbPatches()
    {
        float[] HerbPatchPositionX = new float[4800];
        float[] HerbPatchPositionZ = new float[4800];

        float[,,] MiddleAlpha = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        if (IsThisALoadedGame == false)
        {
            for (int i = 1; i < 4800; i += 4) // we want groups of 4 herbs
            {
                HerbPatchPositionX[i] = Random.Range(0, 9000);
                HerbPatchPositionZ[i] = Random.Range(0, 9000);

                StartCoroutine(RandomHerbPatchPositions(MiddleAlpha, i, HerbPatchPositionX[i], HerbPatchPositionZ[i], 0, 9000, 0, 9000, 4));
            }
        }
    }

    void NetworksInstantiateWaters()
    {
        Transform[] waters = GameObject.Find("WaterList").GetComponentsInChildren<Transform>();

        float[] WaterPositionX = new float[waters.Length];
        float[] WaterPositionZ = new float[waters.Length];

        float[,,] MiddleAlpha = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
        if (IsThisALoadedGame == false)
        {
            for (int i = 1; i < waters.Length; i++)
            {
                WaterPositionX[i] = Random.Range(0, 9000);
                WaterPositionZ[i] = Random.Range(0, 9000);

                StartCoroutine(RandomWaterPositions(MiddleAlpha, i, WaterPositionX[i], WaterPositionZ[i], 0, 9000, 0, 9000));
            }
        }
    }

    void FixedUpdate()
    {
    }
}
