using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleSave : MonoBehaviour
{
    public string uuid;
    public Transform spawnPoint;
    public float rangeTrigger = 5f;
    public Animator animator;
    public bool waitExit = true;
    
    [SerializeField]
    private bool showGizmos = true;
    
    public bool isInteractable
    {
        get
        {
            return PlayerHUD.instance.interactionWith==this;
        }
        set
        {
            if (PlayerHUD.instance.interactionWith==null || PlayerHUD.instance.interactionWith==this)
                PlayerHUD.instance.interactionWith = value ? this : null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position, rangeTrigger);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start capsule"+uuid);
        animator.enabled = false;
        DataSaveValue dataSaveValue = PersistenceDataScene.Instance.dataSaveValue;
        if (dataSaveValue.RuntimeValue.playerPosition == spawnPoint.position)
        {
            Debug.Log("Spawn in this capsule");
            //Spawn in this capsule
            StartCoroutine(SpawnAnimation());
        }
    }
    
    IEnumerator SpawnAnimation()
    {
        Debug.Log("Spawn animation");
        DataSaveValue dataSaveValue = PersistenceDataScene.Instance.dataSaveValue;
        while (ThirdPersonController.instance == null)
        {
            yield return null;
        }
        ThirdPersonController player = ThirdPersonController.instance;
        GroundCollider groundCollider = player.GetComponent<GroundCollider>();
        dataSaveValue.RuntimeValue.playerPosition = Vector3.zero;
        ThirdPersonController.instance.LockInput();
        
        groundCollider.ForceTeleport(spawnPoint.position);
        player.transform.rotation = spawnPoint.rotation;
        
        float time = 0f;
        /*yield return new WaitForSeconds(1f);
        animator.enabled = true;
        yield return new WaitForSeconds(1.8f);*/
        while (time < 2.8f)
        {
            if (Vector3.Distance(player.transform.position, spawnPoint.position) > 0.5f)
            {
                groundCollider.ForceTeleport(spawnPoint.position);
                player.transform.rotation = spawnPoint.rotation;
            }
            time += Time.deltaTime;
            yield return null;
            if (time > 1f && !animator.enabled)
            {
                animator.enabled = true;
            }
        }
        ThirdPersonController.instance.UnlockInput();
    }

    // Update is called once per frame
    void Update()
    {
        if (ThirdPersonController.instance == null) return;
        if (waitExit)
        {
            if (Vector3.Distance(transform.position, ThirdPersonController.instance.transform.position) < rangeTrigger)
                return;
            else
            {
                waitExit = false;
            }
        }
        if (Vector3.Distance(transform.position, ThirdPersonController.instance.transform.position) < rangeTrigger)
        {
            isInteractable = true;
            AddInTeleportList();
        }
        else
        {
            if (isInteractable)
                isInteractable = false;
        }
        if (InputPreset.current.actionInput.GetButtonDown() && isInteractable)
        {
            InteractWith();
        }
    }
    
    public void AddInTeleportList()
    {
        PersistenceDataScene persistenceDataScene = PersistenceDataScene.Instance;
        if (!persistenceDataScene.capsuleMetRuntime.ContainsElement(ListValue.Type.String, uuid))
        {
            persistenceDataScene.capsuleMetRuntime.AddElement(ListValue.Type.String, uuid);
        }
    }
    
    public void InteractWith()
    {
        if (isInteractable)
        {
            OpenSave();
            isInteractable = false;
            waitExit = true;
        }
    }
    
    public void OpenSave()
    {
        PersistenceDataScene persistenceDataScene = PersistenceDataScene.Instance;
        persistenceDataScene.dataSaveValue.RuntimeValue.playerPosition = spawnPoint.position;
        persistenceDataScene.dataSaveValue.RuntimeValue.playerRotation = spawnPoint.rotation;
        persistenceDataScene.OpenOrCloseSavePanel(true);
    }
    
    public void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState != GameState.Gameplay)
        {
            isInteractable = false;
            waitExit = true;
        }
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }
        
    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(CapsuleSave))]
public class CapsuleSaveEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CapsuleSave capsuleSave = (CapsuleSave) target;
        //if uuid is empty, generate a new one
        if (string.IsNullOrEmpty(capsuleSave.uuid))
        {
            capsuleSave.uuid = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(capsuleSave);
        }
    }
}
#endif