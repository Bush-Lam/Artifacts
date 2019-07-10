using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class CharacterMovement : Photon.MonoBehaviour
{
    //7 bosses 7 levels 28 enenemies, 1 map, different textures per map
    //if you kill a certain amount of enemies/reach a certain level boss spawns randomly.
    //redo size of weapons/tools and redo colliders;
    //finish making the skills
    //finish the dynamic terrain
    //make sure multiplayer works
    //make sure rocks/herb patches scale correctly
    //add more audio;
    //add more jump value;
    //FOR ICONS - PLACE TOP OF PREVIEW TO BOTTOM OF MESH RENDERER;

    public Animator Anim;
    public Animator RagdollAnim;
    public Camera mainCamera;
    public float SidetoSideRotation;
    public Vector3 PlayerVelocity;
    Rigidbody playerRigidbody;
    float MovementSpeedElapsed;
    BoxCollider[] AmountofColliders;

    public bool JumpBool;

    public float TimeElapsedJump = 0;
    int DoubleJump = 0;
    public float TimeElapsedTeleport = 0;
    float TimeHeadElapsed = 0; // look at titan
    public GameObject CurrentLookingAtObject;

    TerrainScript terrain;
    MainGUI mainUI;
    CharacterInventoryGUI characterinventory;
    WeaponsDatabase WeaponsDB;
    WeaponSwitch NetworkWeaponAnimations;
    MonsterSpawn mSpawn;
    ChopTrees ChoptreeAnimations;
    MineRocks MinerocksAnimations;
    Herbloring HerbloreAnimations;
    CharacterStats Stats;
    PickupObjects Pickup;
    CharacterSkillBarGUI skillGUI;
    GeneralSkillsDatabase GeneralDB;

    public ArrowSwitch bows;
    public string CurrentElementalStrike; //switch which strike to use by clicking on one of the elemental tabs - default is lmb 
    ElementalSkills EleSkills;
    public List<GameObject> ListofFireStrikes;
    public List<GameObject> ListofIceStrikes;
    public List<GameObject> ListofLightningStrikes;
    public List<GameObject> ListofEarthStrikes;
    public int CurrentStrikeIndex;
    public bool StrikeFromSkillBar;
    public GameObject SwordParticle;

    public Quaternion ProjectileDirection;

    string AnimationName;
    public bool IKActive;
    public float RightHandWeight;
    public float LeftHandWeight;
    public float HeadWeight;

    public float AnimationX;
    public float AnimationY;

    private float animationXTeleportReference;
    public float AnimationXTeleportReference
    {
        get { return animationXTeleportReference; }
        set
        {
            if (TimeElapsedTeleport < Time.time)
                animationXTeleportReference = value;
        }
    }
    private float animationYTeleportReference;
    public float AnimationYTeleportReference
    {
        get { return animationYTeleportReference; }
        set
        {
            if (TimeElapsedTeleport < Time.time)
                animationYTeleportReference = value;
        }
    }

    public float AnimationHandX;
    public float AnimationHandY;
    float AnimationPivot;

    public float TmeAttackElapsed;
    public bool Attacking;

    public AudioSource[] AudioClips; // 0 = walking, 1 = shootingbow, 
    public GameObject RagDollObject;
    public GameObject[] RagDollBones;
    public GameObject[] PlayerBones;
    public bool TransitionAnimationToRagDoll;
    public ParticleSystem[] Wind;

    Vector3 currentup = Vector3.up;
    RaycastHit hit;
    Quaternion targetRotation;
    Vector3 LookPositions = Vector3.zero;

    void OnCollisionEnter(Collision C)
    {

    }

    //IEnumerator StopAttacking()
    //{
    //    if ((Anim.GetCurrentAnimatorStateInfo(2).IsName("StrikeAnim")) &&
    //Anim.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.7f && Attacking == false &&
    //state != RagdollState.blendToAnim && state != RagdollState.ragdolled) // goes back to walking animation from frozen wep animation
    //    {
    //        yield return new WaitForSeconds(0.1f);
    //        SendBools("IsStrike", false);
    //        SendWeights(5, 0);
    //        NetworkSetHandAnimations(null, -1);
    //        TmeAttackElapsed = Time.time + 1 - (Stats.AttackSpeed / 100);
    //    }
    //    else
    //    {
    //        yield return new WaitForSeconds(0.02f);
    //        StartCoroutine("StopAttacking");
    //    }
    //}

    void OnCollisionStay(Collision C)
    {
        if (TimeElapsedJump - 1.7f <= Time.time && !C.transform.CompareTag("Monster") && JumpBool == true &&
            TimeElapsedTeleport - 0.5f <= Time.time)
        {
            for (int i = 0; i < 2; i++)
                Wind[i].Stop();
            JumpBool = false;
            TmeAttackElapsed = Time.time + 0.2f;
            //SendFloats("ArmJump", 1);
            //SendFloats("AttackSpeed", 1);
            TimeElapsedJump = Time.time;

            //StartCoroutine("StopAttacking");
        }
    }

    public void SendFloats(string Name, float Value)
    {
        Anim.SetFloat(Name, Value, 0.0f, Time.deltaTime);
    }

    public void SendInts(string name, int Value)
    {
        Anim.SetInteger(name, Value);
    }

    public void SendBools(string Name, bool Booleans)
    {
        Anim.SetBool(Name, Booleans);
    }

    public void SendWeights(int Value, float Value2)
    {
        Anim.SetLayerWeight(Value, Value2);
    }

    public GameObject AimFinder;

    public IEnumerator TurnOnAnimator(float timeElapsed)
    { // make the ragdoll's skinnedmeshrenderer enabled = false , possibly have everything inside animator ik in charmove have a bool that checks if hit or not

        yield return new WaitForSeconds(0.5f);

        if (Anim.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>().velocity.magnitude < 1 || timeElapsed < Time.time)
        {
            ragdolled = false;
            yield return new WaitForSeconds(0.75f);
            Anim.SetInteger("RagDollState", 0);
            Anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
            transform.GetComponent<Rigidbody>().isKinematic = false;
            transform.GetComponent<Rigidbody>().detectCollisions = true;
            TmeAttackElapsed = Time.time + 1 - (Stats.AttackSpeed / 100);
            yield return new WaitForSeconds(0.75f);
            state = CharacterMovement.RagdollState.animated;
        }
        else
        {
            StartCoroutine(TurnOnAnimator(timeElapsed));
        }
    }

    public void OnAnimatorIK() // for left hand retargeting (weapons)
    {

        //if ((Anim.GetCurrentAnimatorStateInfo(2).IsName("StrikeAnim"))) // this is for keeping jump/attack (2h)animations in sync when neeeded
        //{

        //    if (Anim.GetCurrentAnimatorStateInfo(2).normalizedTime > 0f &&
        //        Anim.GetCurrentAnimatorStateInfo(2).normalizedTime < 0.5f)
        //    {
        //        RightHandWeight = Mathf.Lerp(RightHandWeight, 1, Stats.AttackSpeed / 60 * Time.deltaTime);
        //        Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandWeight / 10);
        //        Anim.SetIKPosition(AvatarIKGoal.RightHand, AimFinder.transform.position);
        //    }
        //    else
        //    {
        //        RightHandWeight = Mathf.Lerp(RightHandWeight, 0, Stats.AttackSpeed / 25 * Time.deltaTime);
        //        Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandWeight / 10);
        //        Anim.SetIKPosition(AvatarIKGoal.RightHand, AimFinder.transform.position);
        //    }
        //}
        //else
        //{
        //    RightHandWeight = Mathf.Lerp(RightHandWeight, 0, Stats.AttackSpeed / 25 * Time.deltaTime);
        //    Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandWeight);
        //}

        //if (bows != null || chakram != null)
        //{
        //    if (Anim.GetCurrentAnimatorStateInfo(1).IsName("Bow"))
        //    {
        //        if (IKActive == true)
        //        {
        //            bows.BowAnim.SetBool("PullBow", true);
        //        }

        //        if (Anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 0.70f)
        //        {

        //            RightHandWeight = Mathf.Lerp(RightHandWeight, 1, Stats.AttackSpeed / 75 * Time.deltaTime);

        //            Anim.SetIKPosition(AvatarIKGoal.RightHand, AimFinder.transform.position);
        //            Anim.SetIKRotation(AvatarIKGoal.LeftHand, NetworkWeaponAnimations.CurrentObject.transform.rotation * Quaternion.Euler(-90, 90, -180));

        //            ProjectileDirection = bows.transform.rotation;

        //            if (bows.BowAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.1f &&
        //                bows.BowAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f &&
        //                bows.BowAnim.GetCurrentAnimatorStateInfo(0).IsName("PullingBow"))
        //            {
        //                LeftHandWeight = Mathf.Lerp(LeftHandWeight, 1, Stats.AttackSpeed / 75 * Time.deltaTime);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftHandWeight);
        //                Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftHandWeight);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandWeight / 25);
        //                Anim.SetIKPosition(AvatarIKGoal.LeftHand, NetworkWeaponAnimations.CurrentObject.transform.Find("Armature").Find("Bone_006").Find("Hand").position);
        //            }
        //            else if (bows.BowAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f &&
        //                bows.BowAnim.GetCurrentAnimatorStateInfo(0).IsName("PullingBow"))
        //            {
        //                LeftHandWeight = Mathf.Lerp(LeftHandWeight, 1, Stats.AttackSpeed / 100 * Time.deltaTime);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        //                Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandWeight / 25);
        //                Anim.SetIKPosition(AvatarIKGoal.LeftHand, NetworkWeaponAnimations.CurrentObject.transform.parent.parent.parent.transform.position +
        //                    NetworkWeaponAnimations.CurrentObject.transform.parent.parent.parent.transform.right / 4 - NetworkWeaponAnimations.CurrentObject.transform.parent.parent.parent.transform.forward / 8.5f
        //                     + NetworkWeaponAnimations.CurrentObject.transform.parent.parent.parent.transform.up / 15);
        //            }
        //            else
        //            {
        //                LeftHandWeight = Mathf.Lerp(LeftHandWeight, 1, Stats.AttackSpeed / 300 * Time.deltaTime);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftHandWeight);
        //                Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftHandWeight);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandWeight / 25);
        //                Anim.SetIKPosition(AvatarIKGoal.LeftHand, NetworkWeaponAnimations.CurrentObject.transform.Find("Armature").Find("Bone_006").Find("Hand").position);
        //            }
        //        }
        //        else
        //        {
        //            if (Anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.70f && Anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 0.95f)
        //            {
        //                LeftHandWeight = Mathf.Lerp(LeftHandWeight, 0, (Stats.AttackSpeed) / 100 * Time.deltaTime);
        //                RightHandWeight = Mathf.Lerp(RightHandWeight, 0, (Stats.AttackSpeed) / 25 * Time.deltaTime);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        //                Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        //                Anim.SetIKPosition(AvatarIKGoal.RightHand, NetworkWeaponAnimations.transform.parent.transform.position);
        //                Anim.SetIKPosition(AvatarIKGoal.LeftHand, NetworkWeaponAnimations.WeaponLeftHand.transform.position);
        //            }
        //            else if (Anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.95f &&
        //                Anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 1)
        //            {
        //                RightHandWeight = Mathf.Lerp(RightHandWeight, 0, (Stats.AttackSpeed) / 100 * Time.deltaTime);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        //                Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        //                TimeHeadElapsed = Time.time + 2;
        //            }
        //        }
        //    }
        //}

        // head rotations

        //if (NetworkWeaponAnimations.CurrentWeaponItemSlot > 0)
        //{
        //    Anim.SetIKPosition(AvatarIKGoal.LeftHand, NetworkWeaponAnimations.CurrentObject.transform.position);
        //    Anim.SetIKRotation(AvatarIKGoal.LeftHand, NetworkWeaponAnimations.CurrentObject.transform.rotation);

        //    Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        //    Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
        //}

        if (Attacking == true)
        {
            if (Anim.GetCurrentAnimatorStateInfo(2).normalizedTime > 0 && Anim.GetCurrentAnimatorStateInfo(2).normalizedTime < 0.65f)
            {
                if (JumpBool == false)
                    HeadWeight = Mathf.Lerp(HeadWeight, 1, 0.33f * Time.deltaTime);
                else
                    HeadWeight = Mathf.Lerp(HeadWeight, 0.75f, 0.33f * Time.deltaTime);

                Anim.SetLookAtPosition(AimFinder.transform.position);
                if (TimeHeadElapsed <= Time.time)
                    TimeHeadElapsed = Time.time + 1;
            }
            else
            {
                // have a gameobject that is set to aimfinder or random mob
                CurrentLookingAtObject = AimFinder.gameObject;
                Anim.SetLookAtPosition(CurrentLookingAtObject.transform.position);
                HeadWeight = Mathf.Lerp(HeadWeight, 0, 0.5f * Time.deltaTime);
            }

            Anim.SetLookAtWeight(HeadWeight);
        }
        else
        {
            if (mSpawn.ListofMonsters.Count < 13)
                return;

            GameObject RandomMob = mSpawn.ListofMonsters[UnityEngine.Random.Range(0, 14)];

            if (Vector3.Angle(transform.forward, RandomMob.transform.position - transform.position) < 45 && TimeHeadElapsed <= Time.time)
            {
                CurrentLookingAtObject = RandomMob;

                Anim.SetLookAtPosition(new Vector3(CurrentLookingAtObject.transform.position.x, CurrentLookingAtObject.GetComponent<MonsterAnimations>().transform.position.y +
                    Vector3.Distance(transform.position, CurrentLookingAtObject.transform.position) / 2, CurrentLookingAtObject.transform.position.z));

                HeadWeight = Mathf.Lerp(HeadWeight, 1, Time.deltaTime / 3);

                if (TimeHeadElapsed + 6 < Time.time)
                    TimeHeadElapsed = Time.time + 1;
            }
            else
            {
                HeadWeight = Mathf.Lerp(HeadWeight, 0, Time.deltaTime / 3);

                if (TimeHeadElapsed <= Time.time)
                    TimeHeadElapsed = Time.time + UnityEngine.Random.Range(3, 10);
                Anim.SetLookAtPosition(CurrentLookingAtObject.transform.position);
            }

            Anim.SetLookAtWeight(HeadWeight);
        }
    }

    IEnumerator LMBElemental()
    {

        if ((Input.GetMouseButton(0) || StrikeFromSkillBar == true) && mainUI.DisableAttackWhenMenuItemPressed == false && Attacking == false &&
            TmeAttackElapsed < Time.time && NetworkWeaponAnimations.CurrentItemId > 0 && CurrentElementalStrike != String.Empty &&
             state != RagdollState.blendToAnim && state != RagdollState.ragdolled && !Anim.GetCurrentAnimatorStateInfo(2).IsName("StrikeAnim"))
        {
            CurrentStrikeIndex++;
            if (CurrentStrikeIndex > 9)
                CurrentStrikeIndex = 0;

            SendBools("IsStrike", true);

            SendFloats("AttackSpeed",  1 + Stats.AttackSpeed / 25);
            NetworkSetHandAnimations("Weapon", 0);
            Attacking = true;
            HeadWeight = 0;

            EleSkills.ElementalStrikes(CurrentElementalStrike + " " + "Strike");

            AnimationHandX = Mathf.Lerp(AnimationHandX, 0, 10 * Time.deltaTime);
            AnimationHandY = Mathf.Lerp(AnimationHandY, 0, 10 * Time.deltaTime);
            SendFloats("HandX", AnimationHandX);
            SendFloats("HandY", AnimationHandY);

            Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 35));

            AimFinder.transform.position = p;

            SendFloats("HandRotationY", (Input.mousePosition.y / Screen.height) * 2.5f);
            SendFloats("HandRotationX", Input.mousePosition.x / Screen.width);

            EleSkills.StrikeParticle.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            yield return new WaitForSeconds(0.75f);
            if (JumpBool == true)
                SendWeights(5, 0);

            LeftHandWeight = 0;

            Stats.CurrentPlayerStamina -= Mathf.Round(Stats.CurrentPlayerStamina * 0.01f);

            if (CurrentElementalStrike == "Fire")
                EleSkills.ShootElemental("Fire Strike", 1750);
            else if (CurrentElementalStrike == "Ice")
                EleSkills.ShootElemental("Ice Strike", 1750);
            else if (CurrentElementalStrike == "Lightning")
                EleSkills.ShootElemental("Lightning Strike", 5000);
            else if (CurrentElementalStrike == "Earth")
                EleSkills.ShootElemental("Earth Strike", 1750);

            //yield return new WaitForSeconds(0.3f);

            yield return new WaitForSeconds(1.45f - Stats.AttackSpeed / 25);

            //if (JumpBool == false || Anim.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.8f) // freeze staff animation when in the middle of a jump
            //{
            //    SendBools("IsStrike", false);
            //    SendWeights(5, 0);
            //    NetworkSetHandAnimations(null, -1);
            //}

            SendBools("IsStrike", false);
            SendWeights(5, 0);
            NetworkSetHandAnimations(null, -1);

            //Debug.Log(1 - (Stats.AttackSpeed / 100));

            yield return new WaitForSeconds(0.2f);

            TmeAttackElapsed = Time.time + 1 - (Stats.AttackSpeed / 25);
            Attacking = false;
            StrikeFromSkillBar = false;
        }

        yield return new WaitForSeconds(0.02f);
        StartCoroutine("LMBElemental");
    }

    public IEnumerator ChargeAttack()
    {
        if (Attacking == true)
        {
            if (Anim.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.4f && Anim.GetCurrentAnimatorStateInfo(2).IsName("ChargeAnim")) // freeze wep animation 
            {
                SendFloats("AttackSpeed", 0);
            }

            AnimationHandX = Mathf.Lerp(AnimationHandX, 0, 10 * Time.deltaTime);
            AnimationHandY = Mathf.Lerp(AnimationHandY, 0, 10 * Time.deltaTime);
            SendFloats("HandX", AnimationHandX);
            SendFloats("HandY", AnimationHandY);

            Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 35));

            AimFinder.transform.position = p;

            if (TmeAttackElapsed > Time.time) // charging up atk
            {
                EleSkills.StrikeParticle.transform.localScale = Vector3.Lerp(EleSkills.StrikeParticle.transform.localScale, new Vector3(0.6f, 0.6f, 0.6f), 3 * Time.deltaTime * (0.115f / (3.7f - (3.7f * (Stats.AttackSpeed / 100)))));

                if (Input.GetKey(KeyCode.Escape))
                {
                    SendBools("IsCharging", false);
                    NetworkSetHandAnimations(null, -1);
                    SendWeights(5, 0);
                    mainUI.CheckMovementProgressBar = true;
                    TmeAttackElapsed = Time.time + 1;
                    EleSkills.StrikeParticle.transform.SetParent(null); // temp

                    yield return new WaitForSeconds(0.1f);
                    Attacking = false;
                    StartCoroutine("ChargeAttack");
                    yield break;
                }
            }

            if (TmeAttackElapsed < Time.time && Input.GetMouseButton(0)) // ready to atk
            {
                SendFloats("AttackSpeed", 1.2f);
                yield return new WaitForSeconds(0.5f);
                if (JumpBool == true)
                    SendWeights(5, 0);

                LeftHandWeight = 0;

                Stats.CurrentPlayerStamina -= Mathf.Round(Stats.CurrentPlayerStamina * 0.01f);

                // charging Attack
                //if (CurrentElementalStrike == "Fire")
                //    EleSkills.ShootElemental("Fire Strike", 1750);
                //else if (CurrentElementalStrike == "Ice")
                //    EleSkills.ShootElemental("Ice Strike", 1750);
                //else if (CurrentElementalStrike == "Lightning")
                //    EleSkills.ShootElemental("Lightning Strike", 5000);
                //else if (CurrentElementalStrike == "Earth")
                //    EleSkills.ShootElemental("Earth Strike", 1750);

                yield return new WaitForSeconds(0.2f);
                mainUI.CheckMovementProgressBar = true;

                //yield return new WaitForSeconds((100 - Stats.AttackSpeed) / (Stats.AttackSpeed * 2.5f));
                SendBools("IsCharging", false);
                SendWeights(5, 0);
                NetworkSetHandAnimations(null, -1);

                TmeAttackElapsed = Time.time + 1;

                yield return new WaitForSeconds(0.2f);

                Attacking = false;
            }
            yield return new WaitForSeconds(0.2f);
            StartCoroutine("ChargeAttack");
        }
    }

    public void ChargeAttackAnimation()
    {
        if (NetworkWeaponAnimations.CurrentObject.name.Contains("Staff") && !Anim.GetCurrentAnimatorStateInfo(2).IsName("ChargeAnim"))
        {
            //CurrentStrikeIndex++; // chargeindex instead
            //if (CurrentStrikeIndex > 5)
            //    CurrentStrikeIndex = 0;

            mainUI.timeElapsedProgressBar = 4.9f + Time.time - (4.9f * (Stats.AttackSpeed / 100));
            TmeAttackElapsed = Time.time + 3.7f;

            mainUI.StartCoroutine(mainUI.ActivateProgressBar(0.215f / 3.7f, CurrentElementalStrike + " Strike"));
            mainUI.CheckMovementProgressBar = false;
            //SendBools("IsStrike", true); // isCharging
            SendFloats("AttackSpeed", 1.2f);
            NetworkSetHandAnimations("Weapon", 0);
            Attacking = true;

            //EleSkills.ElementalStrikes(CurrentElementalStrike + " " + "Strike"); // chargeAnim
        }
    }

    public void NetworkSetHandAnimations(string TypeHand, int state)
    {
        /*if (IsSwinging == 2 && IsBow == 2)*/

        if (TypeHand == "Gathering")
        {
            SendInts("GatheringStates", state);
        }

        if (TypeHand == "Crafting")
        {
            SendInts("CraftingStates", state);
        }

        if (state == 0 && TypeHand == "Weapon") //1 hand
        {
            SendWeights(4, 1);
            SendWeights(5, 1);
            SendWeights(7, 0);
            SendWeights(6, 1);
            SendWeights(2, 1);
            SendWeights(3, 0);
            SendWeights(1, 1);
            SendWeights(8, 0);
        }
        else if ((state == 1 && TypeHand == "Weapon") || ((TypeHand == "Gathering" || TypeHand == "Crafting") &&
            (state == 1 || state == 3 || state == 2 || state == 4 || state == 5))) //everything involving 2 hands
        {
            SendWeights(4, 1);
            SendWeights(5, 1);
            SendWeights(7, 0);
            SendWeights(6, 1);
            SendWeights(2, 1);
            SendWeights(1, 1);
            SendWeights(8, 0);
        }
        else if (NetworkWeaponAnimations.CurrentItemId > 0)
        {
            SendWeights(6, 0);
            SendWeights(2, 1);
            SendWeights(1, 1);
            SendWeights(8, 1);
            SendWeights(7, 1);
            SendWeights(5, 0);
            SendWeights(4, 0);
            SendWeights(3, 0);
        }
        else // strike left animation - make two animations one for forward, other for left/right 
        {
            SendWeights(7, 1);
            SendWeights(8, 0);
            SendWeights(4, 0);
            SendWeights(5, 0);
            SendWeights(6, 0);
            SendWeights(1, 1);
            SendWeights(2, 1);
            SendWeights(3, 0);
        }
    }

    void IdleRotation()
    {
        //put rotationangle into arms animator

        if (Input.GetMouseButton(1) && SidetoSideRotation != 0 && PlayerVelocity == Vector3.zero) // fix
        {
            if (SidetoSideRotation < 0)
            {
                AnimationPivot = Mathf.Lerp(AnimationPivot, -2, Time.deltaTime * 2);
                SendBools("IsMoving", true);
                SendFloats("RotationAngle", AnimationPivot);
            }
            else if (SidetoSideRotation > 0)
            {
                AnimationPivot = Mathf.Lerp(AnimationPivot, 2, Time.deltaTime * 2);
                SendBools("IsMoving", true);
                SendFloats("RotationAngle", AnimationPivot);
            }
        }
        else if (AnimationPivot != 0)
        {
            AnimationPivot = Mathf.Lerp(AnimationPivot, 0, Time.deltaTime * 2);
            SendFloats("RotationAngle", AnimationPivot);
        }
    }

    void NormalToSurface()
    {
        targetRotation = playerRigidbody.rotation;

        if (Physics.Raycast(playerRigidbody.position, -Vector3.up, out hit, 10))
        {
            currentup = hit.normal;
            LookPositions = Vector3.Cross(currentup, Vector3.Cross(transform.forward, currentup));
            targetRotation = Quaternion.LookRotation(LookPositions.normalized, currentup);
            playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, targetRotation, Time.deltaTime);
        }
    }

    void MultiplayerCharacterMovement()
    {
        if (Input.GetKey(mainUI.KeyBinds[5]) && Time.time > TimeElapsedTeleport)
        {
            transform.GetComponentInChildren<ParticleSystem>().transform.rotation = transform.rotation * Quaternion.LookRotation(new Vector3(AnimationXTeleportReference, 0, AnimationYTeleportReference), Vector3.up);
            TimeElapsedTeleport = Time.time + 0.5f;
            mainCamera.GetComponent<CameraScript>().IncreasingTime = 0;
            StartCoroutine("Teleport");
            photonView.RPC("TeleportMultiplayer", PhotonTargets.All, transform.GetComponentInChildren<ParticleSystem>().GetComponent<PhotonView>().viewID);
            Stats.CurrentPlayerStamina -= Stats.PlayerStamina * 0.05f;
        }

        if (Input.GetKey(KeyCode.LeftShift)
           /* Stats.CurrentPlayerStamina > Stats.CurrentPlayerStamina * 0.1f*/) // running only in test build 
        {
            if (Time.time > MovementSpeedElapsed)
            {
                MovementSpeedElapsed = Time.time + 1;
                //Stats.CurrentPlayerStamina -= Stats.CurrentPlayerStamina * 0.1f;
            }
            transform.position = new Vector3(transform.position.x, terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y + 0.05f, transform.position.z);
            Stats.MovementSpeed = 350;
            SendFloats("SpeedValue", Stats.MovementSpeed + 2);
        }
        else if (Time.time > MovementSpeedElapsed)
        {
            if (GetComponentInChildren<Rigidbody>().velocity.y > 20)
                GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;
            MovementSpeedElapsed = Time.time + 0.5f;
            if (GetComponentInChildren<Rigidbody>().velocity.magnitude > 0.1f)
                SendFloats("SpeedValue", Stats.MovementSpeed + 2);
            else
                SendFloats("SpeedValue", 0);
        }

        // autorun logic in mainGUI

        if (Input.GetKey(mainUI.KeyBinds[0]) && Input.GetKey(mainUI.KeyBinds[1]))
        {
            AnimationX = Mathf.Lerp(AnimationX, -1.0f, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, 1.0f, 20 * Time.deltaTime);
            AnimationXTeleportReference = -1;
            AnimationYTeleportReference = 1;
        }

        else if (Input.GetKey(mainUI.KeyBinds[0]) && Input.GetKey(mainUI.KeyBinds[2]))
        {
            AnimationX = Mathf.Lerp(AnimationX, 1.0f, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, 1.0f, 20 * Time.deltaTime);
            AnimationXTeleportReference = 1;
            AnimationYTeleportReference = 1;
        }

        else if (Input.GetKey(mainUI.KeyBinds[3]) && Input.GetKey(mainUI.KeyBinds[1]))
        {
            AnimationX = Mathf.Lerp(AnimationX, -1.0f, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, -1.0f, 20 * Time.deltaTime);
            AnimationXTeleportReference = -1;
            AnimationYTeleportReference = -1;
        }

        else if (Input.GetKey(mainUI.KeyBinds[3]) && Input.GetKey(mainUI.KeyBinds[2]))
        {
            AnimationX = Mathf.Lerp(AnimationX, 1.0f, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, -1.0f, 20 * Time.deltaTime);
            AnimationXTeleportReference = 1;
            AnimationYTeleportReference = -1;
        }

        else if (Input.GetKey(mainUI.KeyBinds[0]) || mainUI.AutoRun == true) // forward
        {
            AnimationX = Mathf.Lerp(AnimationX, 0, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, 1.0f, 20 * Time.deltaTime);
            AnimationXTeleportReference = 0;
            AnimationYTeleportReference = 1;
        }

        else if (Input.GetKey(mainUI.KeyBinds[3]))
        {
            AnimationX = Mathf.Lerp(AnimationX, 0, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, -1.0f, 20 * Time.deltaTime);
            AnimationXTeleportReference = 0;
            AnimationYTeleportReference = -1;
        }

        else if (Input.GetKey(mainUI.KeyBinds[2]))
        {
            AnimationX = Mathf.Lerp(AnimationX, 1.0f, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, 0, 20 * Time.deltaTime);
            AnimationXTeleportReference = 1;
            AnimationYTeleportReference = 0;
        }

        else if (Input.GetKey(mainUI.KeyBinds[1]))
        {
            AnimationX = Mathf.Lerp(AnimationX, -1.0f, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, 0, 20 * Time.deltaTime);
            AnimationXTeleportReference = -1;
            AnimationYTeleportReference = 0;
        }

        else if (mainUI.AutoRun == false)
        {
            /*				AudioClips[0].Stop();*/

            AnimationX = Mathf.Lerp(AnimationX, 0, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, 0, 20 * Time.deltaTime);
            AnimationXTeleportReference = 0;
            AnimationYTeleportReference = 1;
        }
        else
        {
            AnimationX = Mathf.Lerp(AnimationX, 0, 20 * Time.deltaTime);
            AnimationY = Mathf.Lerp(AnimationY, 0, 20 * Time.deltaTime);
            AnimationXTeleportReference = 0;
            AnimationYTeleportReference = 1;
        }

        if (Attacking == false)
        {
            AnimationHandX = Mathf.Lerp(AnimationX, 0, 20 * Time.deltaTime);
            AnimationHandY = Mathf.Lerp(AnimationY, 0, 20 * Time.deltaTime);
            SendFloats("HandX", AnimationHandX);
            SendFloats("HandY", AnimationHandY);
        }

        if (JumpBool == false)
        {
            PlayerVelocity = new Vector3(0, 0, AnimationY) + new Vector3(AnimationX, 0, 0);
            PlayerVelocity = transform.localRotation * PlayerVelocity * Stats.MovementSpeed;

            if (PlayerVelocity.magnitude < 1)
            {
                SendBools("IsIdle", true);
                SendBools("IsMoving", false);
                SendBools("IsJumping", false);
                SendBools("IsDoubleJump", false);
                SendBools("IsDash", false);
            }
            else
            {
                
                SendBools("IsIdle", false);
                SendBools("IsMoving", true);
                SendBools("IsJumping", false);
                SendBools("IsDoubleJump", false);
                SendBools("IsDash", false);
            }

            if (PlayerVelocity.y != 0)
                PlayerVelocity.y = GetComponentInChildren<Rigidbody>().velocity.y; // max height

            if (Vector3.Distance(transform.position, new Vector3(transform.position.x, terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y, transform.position.z)) < 0.5f)
                playerRigidbody.position = Vector3.Slerp(playerRigidbody.position, new Vector3(playerRigidbody.position.x, terrain.terrain.SampleHeight(playerRigidbody.position) +
                terrain.transform.position.y, playerRigidbody.position.z), Time.deltaTime / 2); // keep player grounded
            else
                PlayerVelocity.y = GetComponentInChildren<Rigidbody>().velocity.y;

            if (Vector3.Distance(transform.position, terrain.MasterPosition.transform.position) < 2000 && state == RagdollState.animated) // if other players are too far away(maybe within fog distance), then stop moving 
                playerRigidbody.velocity = PlayerVelocity;
            else
                playerRigidbody.velocity = Vector3.zero;
        }

        SendFloats("posX", AnimationX);
        SendFloats("posY", AnimationY);
        SendFloats("HandRotationY", (Input.mousePosition.y / Screen.height) * 2.5f);
        SendFloats("HandRotationX", -1);

        if (TimeElapsedTeleport >= Time.time)
        {
            playerRigidbody.velocity = transform.rotation * new Vector3(AnimationXTeleportReference, 0, AnimationYTeleportReference) * 10;

            if (Vector3.Distance(transform.position, new Vector3(transform.position.x, terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y, transform.position.z)) < 0.5f)
                playerRigidbody.position = Vector3.Slerp(playerRigidbody.position, new Vector3(playerRigidbody.position.x, terrain.terrain.SampleHeight(playerRigidbody.position) +
                terrain.transform.position.y, playerRigidbody.position.z), Time.deltaTime / 2); // keep player grounded
            else
                PlayerVelocity.y = GetComponentInChildren<Rigidbody>().velocity.y;
        }

        if (DoubleJump > 0 && JumpBool == true)
        {
            if (Input.GetKey(mainUI.KeyBinds[4]) && TimeElapsedJump - 2.5f <= Time.time  && state == RagdollState.animated)
            {
                DoubleJump++;
                JumpBool = true;

                if (DoubleJump > 1) // replay double jump
                {
                    Anim.Play("DoubleJumping", 1, 0);
                    Anim.Play("DoubleJumping", 2, 0);
                    Anim.Play("DoubleJumping", 4, 0);
                    Anim.Play("DoubleJumping", 6, 0);
                    Anim.Play("DoubleJumping", 7, 0);

                    if (Attacking == false)
                    {
                        //Debug.Log(Attacking);
                        //Anim.Play("DoubleJumping", 6, 0);
                        //Anim.Play("DoubleJumping", 7, 0);
                    }
                }
                if (DoubleJump > 20)
                    DoubleJump = 0;

                StartCoroutine(Jumping(true));

            }
        }
        else
        {
            if (Input.GetKey(mainUI.KeyBinds[4]) &&
                JumpBool == false && TimeElapsedJump + 0.5f <= Time.time && state == RagdollState.animated)
            {
                DoubleJump++;
                JumpBool = true;
                StartCoroutine(Jumping(false));
            }
        }
    }

    [PunRPC]
    void TeleportMultiplayer(int ID)
    {
        PhotonView trail = PhotonView.Find(ID);
        trail.GetComponentInChildren<ParticleSystem>().Stop();
        trail.GetComponentInChildren<ParticleSystem>().Play(); // teleport trail
    }

    IEnumerator Teleport()
    {
        TmeAttackElapsed = Time.time + 0.2f;
        yield return new WaitForSeconds(0.02f);
        for (int i = 0; i < 2; i++)
            Wind[i].Play();

        if (Anim.GetCurrentAnimatorStateInfo(4).IsName("Dash"))
        {
            Anim.Play("Dash", 1, 0);
            Anim.Play("Dash", 2, 0);
            Anim.Play("Dash", 4, 0);
            Anim.Play("Dash", 6, 0);
            Anim.Play("Dash", 7, 0);
        }

        Anim.SetFloat("Jumpx", AnimationX);
        Anim.SetFloat("Jumpy", AnimationY);

        Anim.SetBool("IsJumping", false);
        Anim.SetBool("IsDoubleJump", false);
        Anim.SetBool("IsDash", true);

        //playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x / 2, playerRigidbody.velocity.y, playerRigidbody.velocity.z / 2);
        //playerRigidbody.AddForce(((transform.rotation * new Vector3(AnimationXTeleportReference, 0, AnimationYTeleportReference) * 1.25f)) * 12, ForceMode.VelocityChange);


        //playerRigidbody.position = transform.rotation * new Vector3(AnimationXTeleportReference, 0, AnimationYTeleportReference) * 3.25f;
        //float terrainHeight = terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y;

        //if (JumpBool == false)
        //    playerRigidbody.position = new Vector3(playerRigidbody.position.x, terrainHeight, playerRigidbody.position.z);
    }

    IEnumerator Jumping(bool doubleJump)
    {
        TmeAttackElapsed = Time.time + 0.2f;

        photonView.RPC("TeleportMultiplayer", PhotonTargets.All, transform.GetComponentInChildren<ParticleSystem>().GetComponent<PhotonView>().viewID);
        transform.GetComponentInChildren<ParticleSystem>().transform.rotation = Anim.GetBoneTransform(HumanBodyBones.Hips).transform.rotation * Quaternion.Euler(0,90,0);

        TimeElapsedJump = Time.time + 3;

        Anim.SetBool("IsIdle", false);
        Anim.SetBool("IsMoving", false);
        for (int i = 0; i < 2; i++)
            Wind[i].Play();

        if (doubleJump == true)
        {
            Anim.SetBool("IsJumping", false);
            Anim.SetBool("IsDoubleJump", true);
            Anim.SetBool("IsDash", false);
        }
        else
        {
            Anim.SetBool("IsJumping", true);
            Anim.SetBool("IsDoubleJump", false);
            Anim.SetBool("IsDash", false);
        }

        Anim.SetFloat("Jumpx", AnimationX);
        Anim.SetFloat("Jumpy", AnimationY);

        yield return new WaitForSeconds(0);
        playerRigidbody.isKinematic = false;
        playerRigidbody.velocity = Vector3.zero;
        playerRigidbody.AddForce(((Vector3.up * 2.5f + transform.rotation * new Vector3(AnimationXTeleportReference, 0, AnimationYTeleportReference) * 1.25f)) * 4, ForceMode.VelocityChange);

        StopCoroutine("Jumping");
    }

    void Start()
    {
        terrain = GameObject.FindWithTag("MainEnvironment").GetComponentInChildren<TerrainScript>();
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
        mainUI = terrain.canvas.GetComponentInChildren<MainGUI>();
        characterinventory = terrain.canvas.GetComponentInChildren<CharacterInventoryGUI>();
        Anim = gameObject.GetComponent<Animator>();
        RagdollAnim = RagDollObject.GetComponentInChildren<Animator>();
        mSpawn = terrain.GetComponent<MonsterSpawn>();
        NetworkWeaponAnimations = gameObject.GetComponentInChildren<WeaponSwitch>();
        ChoptreeAnimations = gameObject.GetComponentInChildren<ChopTrees>();
        MinerocksAnimations = gameObject.GetComponentInChildren<MineRocks>();
        HerbloreAnimations = gameObject.GetComponentInChildren<Herbloring>();
        Stats = gameObject.GetComponentInChildren<CharacterStats>();
        WeaponsDB = gameObject.GetComponentInChildren<WeaponsDatabase>();
        Pickup = gameObject.GetComponentInChildren<PickupObjects>();
        EleSkills = gameObject.GetComponentInChildren<ElementalSkills>();
        skillGUI = terrain.canvas.GetComponentInChildren<CharacterSkillBarGUI>();
        GeneralDB = gameObject.GetComponentInChildren<GeneralSkillsDatabase>();
        mainCamera = gameObject.GetComponentInChildren<Camera>();

        Anim.SetLayerWeight(1, 1);
        NetworkSetHandAnimations("null", -1);

        TimeElapsedJump = TimeElapsedJump + Time.time + 1;
        TmeAttackElapsed = Time.time + Stats.AttackSpeed;
        CurrentLookingAtObject = transform.gameObject;

        StartCoroutine("LMBElemental");

        //ragdoll logic 
        Collider[] CollidersPlayer = transform.Find("Chest_001").GetComponentsInChildren<Collider>();
        Collider[] ColliderRagDoll = RagDollObject.transform.GetComponentsInChildren<Collider>();
        PlayerBones = new GameObject[CollidersPlayer.Length];
        RagDollBones = new GameObject[ColliderRagDoll.Length];
        for (int x = 0; x < CollidersPlayer.Length; x++)
        {
            PlayerBones[x] = CollidersPlayer[x].gameObject;
            RagDollBones[x] = ColliderRagDoll[x].gameObject;
            RagDollBones[x].GetComponent<Rigidbody>().isKinematic = true;
        }
        RagDollObject.transform.SetParent(null);
        RagDollObject.SetActive(false);
        SendInts("RagDollState", 0);

        //Find all the transforms in the character, assuming that this script is attached to the root
        Component[] components = GetComponentsInChildren(typeof(Transform));

        //For each of the transforms, create a BodyPart instance and store the transform 
        foreach (Component c in components)
        {
            if (c.GetComponent<Rigidbody>() != null && c.GetComponent<PhotonView>() == null &&
                c != transform)
            {
                BodyPart bodyPart = new BodyPart();
                bodyPart.transform = c as Transform;
                bodyParts.Add(bodyPart);
            }
        }
        setKinematic(true);

    }

    //void OnAnimatorMove()
    //{
    //    Animator animator = GetComponent<Animator>();

    //    if (animator && Time.time >= TimeElapsedJump)
    //    {
    //        Vector3 newPosition = transform.position;
    //        newPosition.x += PlayerVelocity.x * 2 * Time.deltaTime;
    //        newPosition.z += PlayerVelocity.z * 2 * Time.deltaTime;
    //        newPosition.y = terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y + 0.5f;
    //        //transform.position = Vector3.Slerp(transform.position, new Vector3(transform.position.x, terrain.terrain.SampleHeight(transform.position) +
    //        //terrain.transform.position.y + 0.5f, transform.position.z), Time.deltaTime / 2); // keep player grounded
    //        transform.position = newPosition;
    //    }
    //}

    public bool ragdolled
    {
        get
        {
            return state != RagdollState.animated;
        }
        set
        {
            if (value == true)
            {
                if (state == RagdollState.animated)
                {
                    //Transition from animated to ragdolled
                    setKinematic(false); //allow the ragdoll RigidBodies to react to the environment
                    Anim.enabled = false; //disable animation
                    state = RagdollState.ragdolled;
                }
            }
            else
            {
                if (state == RagdollState.ragdolled)
                {
                    //Transition from ragdolled to animated through the blendToAnim state
                    setKinematic(true); //disable gravity etc.
                    ragdollingEndTime = Time.time; //store the state change time
                    Anim.enabled = true; //enable animation
                    state = RagdollState.blendToAnim;

                    //Store the ragdolled position for blending
                    foreach (BodyPart b in bodyParts)
                    {
                        b.storedRotation = b.transform.rotation;
                        b.storedPosition = b.transform.position;
                    }

                    //Remember some key positions
                    ragdolledFeetPosition = 0.5f * (Anim.GetBoneTransform(HumanBodyBones.LeftToes).position + Anim.GetBoneTransform(HumanBodyBones.RightToes).position);
                    ragdolledHeadPosition = Anim.GetBoneTransform(HumanBodyBones.Head).position;
                    ragdolledHipPosition = Anim.GetBoneTransform(HumanBodyBones.Hips).position;

                    //Initiate the get up animation
                    if (Vector3.Dot(Anim.GetBoneTransform(HumanBodyBones.Chest).transform.up, Vector3.down) > 0) //hip hips forward vector pointing upwards, initiate the get up from back animation
                    {
                        Anim.SetInteger("RagDollState", 2);
                    }
                    else
                    {
                        Anim.SetInteger("RagDollState", 1);
                    }
                } //if (state==RagdollState.ragdolled)
            }   //if value==false	
        } //set
    }

    public enum RagdollState
    {
        animated,    //Mecanim is fully in control
        ragdolled,   //Mecanim turned off, physics controls the ragdoll
        blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
    }

    //The current state
    public RagdollState state = RagdollState.animated;

    //How long do we blend when transitioning from ragdolled to animated
    public float ragdollToMecanimBlendTime = 3;
    float mecanimToGetUpTransitionTime = 0.05f;

    //A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
    public float ragdollingEndTime = -100;

    //Declare a class that will hold useful information for each body part
    public class BodyPart
    {
        public Transform transform;
        public Vector3 storedPosition;
        public Quaternion storedRotation;
    }
    //Additional vectores for storing the pose the ragdoll ended up in.
    Vector3 ragdolledHipPosition, ragdolledHeadPosition, ragdolledFeetPosition;

    //Declare a list of body parts, initialized in Start()
    public List<BodyPart> bodyParts = new List<BodyPart>();

    public void setKinematic(bool newValue)
    {
        //Get an array of components that are of type Rigidbody
        Component[] components = GetComponentsInChildren(typeof(Rigidbody));

        //For each of the components in the array, treat the component as a Rigidbody and set its isKinematic property
        foreach (Component c in components)
        {
            if (c.gameObject != this.gameObject && c.GetComponent<PhotonView>() == null &&
                c != transform)
            {
                (c as Rigidbody).isKinematic = newValue;
                //(c as Rigidbody).detectCollisions = !newValue;
            }
            //(c as Rigidbody).detectCollisions = !newValue;
        }
    }

    private void LateUpdate() // copy paste all ragdollhelper 
    {
        //Blending from ragdoll back to animated
        if (state == RagdollState.blendToAnim)
        {
            if (Time.time <= ragdollingEndTime + ragdollToMecanimBlendTime)
            {
                //If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
                //character to the best match with the ragdoll
                Vector3 animatedToRagdolled = ragdolledHipPosition - Anim.GetBoneTransform(HumanBodyBones.Hips).position;
                Vector3 newRootPosition = transform.position + animatedToRagdolled;

                //Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
                //RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition, Vector3.down));
                newRootPosition.y = terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y;

                //foreach (RaycastHit hit in hits)
                //{
                //    if (!hit.transform.IsChildOf(transform))
                //    {
                //        newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
                //    }
                //}
                transform.position = newRootPosition;

                //Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
                Vector3 ragdolledDirection = ragdolledHeadPosition - ragdolledFeetPosition;
                ragdolledDirection.y = 0;

                Vector3 meanFeetPosition = 0.5f * (Anim.GetBoneTransform(HumanBodyBones.LeftFoot).position + Anim.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 animatedDirection = Anim.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
                animatedDirection.y = 0;

                //Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
                //hence setting the y components of the vectors to zero. 
                transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdolledDirection.normalized);
                //Vector3 newDir = Vector3.RotateTowards(animatedDirection.normalized, ragdolledDirection.normalized, Time.deltaTime, 0);

                //transform.rotation = Quaternion.Slerp(transform.rotation, Camera.main.transform.rotation, Time.deltaTime);
            }

            //compute the ragdoll blend amount in the range 0...1
            float ragdollBlendAmount = 1.0f - (Time.time - ragdollingEndTime - mecanimToGetUpTransitionTime) / ragdollToMecanimBlendTime;
            ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

            //In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
            //To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
            //and slerp all the rotations towards the ones stored when ending the ragdolling
            foreach (BodyPart b in bodyParts)
            {
                if (b.transform != transform)
                { //this if is to prevent us from modifying the root of the character, only the actual body parts
                  //position is only interpolated for the hips
                    if (b.transform == Anim.GetBoneTransform(HumanBodyBones.Hips))
                        b.transform.position = Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
                    //rotation is interpolated for all body parts
                    b.transform.rotation = Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
                }
            }

            //if the ragdoll blend amount has decreased to zero, move to animated state
            if (ragdollBlendAmount == 0)
            {
                Anim.SetInteger("RagDollState", 0);
                return;
            }
        }
    }

    void FixedUpdate()
    {
        //if (terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y - transform.position.y < 0 && JumpBool == false && state == RagdollState.animated)
        //    playerRigidbody.position = new Vector3(transform.position.x, terrain.terrain.SampleHeight(transform.position) + terrain.transform.position.y, transform.position.z);
    }

    void Update()
    {
        //if (Anim.GetCurrentAnimatorStateInfo(2).IsName("StrikeAnim") &&
        //    Anim.GetCurrentAnimatorStateInfo(2).normalizedTime % 1 > 0.9f &&
        //    (JumpBool == true || TimeElapsedTeleport > Time.time))
        //    SendFloats("AttackSpeed", 0);

        if (photonView.isMine && Anim != null)
        {

            if (photonView.isMine && Anim != null && state == RagdollState.animated &&
    state != RagdollState.ragdolled && state != RagdollState.blendToAnim)
            {

                if (Input.GetMouseButton(1))
                {
                    SidetoSideRotation = Input.GetAxis("Mouse X") * 2 * mainUI.Sensitivity;
                    transform.Rotate(0, SidetoSideRotation, 0);
                }
                else if (SidetoSideRotation != 0)
                    SidetoSideRotation = 0;

                MultiplayerCharacterMovement(); // vector3.lerp position to terrain, remove collision with mainterrain.
            }

            IdleRotation();
            NormalToSurface();
        }
    }
}
