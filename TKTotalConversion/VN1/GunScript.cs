using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using Zorro.Core.Serizalization;
using Random = UnityEngine.Random;

namespace TKTotalConversion.VN1;

public class ChargeDisplay : MonoBehaviour {
    public Transform[] Charges;
    public SFX_PlayOneShot ChargeSFX;
}

public class Scope : MonoBehaviour {
    public Camera Camera;
    public MeshRenderer CameraScreen;
}

public class GunData : ItemDataEntry {
    public 
    public override void Serialize(BinarySerializer binarySerializer)
    {
        
    }

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        
    }
}
public class Gun : ItemInstanceBehaviour {
    public GameObject BulletPrefab;
    public Transform BulletSpawnpoint;
    public ChargeDisplay ChargeDisplay;
    public Scope WeaponScope;

    public GameObject[] EnabledWhileEquipped; 
    public GameObject[] DisabledWhileEquipped;
    
    public override void ConfigItem(ItemInstanceData data, PhotonView playerView)
    {
        
        
        itemInstance.onItemEquipped.AddListener(OnItemEquipped);
        itemInstance.onUnequip.AddListener(OnItemUnequipped);
    }

    private void OnItemEquipped(Player byPlayer)
    {
        foreach (var o in EnabledWhileEquipped)
            o.SetActive(true);
        foreach (var o in DisabledWhileEquipped)
            o.SetActive(false);
    }

    private void OnItemUnequipped(Player byPlayer)
    {
        foreach (var o in EnabledWhileEquipped)
            o.SetActive(false);
        foreach (var o in DisabledWhileEquipped)
            o.SetActive(true);
    }
}

public class VN1Rifle : ItemInstanceBehaviour {
    public GameObject BulletProjectile;
    public Transform BarrelTip;
    public Transform[] Charges;
    public SFX_PlayOneShot ChargeSFX;
    public Camera Camera;
    public MeshRenderer CameraScreen;
    public Collider UpperStock;

    public BatteryDisplay FlashlightBattery;
    public Light Flashlight;
    public GameObject FlashlightSignaler;
    
    private const float CameraFOV = 60f;
    private const float FlashlightDepletionTime = 10f;
    private const float FlashlightRegenTime = 15f;
    private const float ChargeDepletionTime = 1.5f;
    private const float ChargeRegenTime = 1f;
    private int lastCharge = -1;
    private float cameraZoom = 0f;
    private VN1RifleData? data;
    private int fireRate = 6;
    private float timer = 0f;
    private float chargeRegenTimer = 0f;
    private float lastFlashlightCharge = -1f;
    public override void ConfigItem(ItemInstanceData instanceData, PhotonView playerView)
    {
        if (instanceData.TryGetEntry<VN1RifleData>(out data))
        {
            TKTotalConversion.Logger.LogDebug($"Found VN1Rifle Data: {data.ShotsFired}");
        } else
        {
            data = new VN1RifleData(Charges.Length);
            instanceData.AddDataEntry(data);
            TKTotalConversion.Logger.LogDebug("No VN1Rifle Data found. Creating new!");
        }

        UpperStock.enabled = false;
        
        itemInstance.onItemEquipped.AddListener(ply =>
        {
            UpperStock.enabled = false;
            EnableCameraView(ply.IsLocal);
        });
        itemInstance.onUnequip.AddListener(ply => UpperStock.enabled = true);

        itemInstance.RegisterRPC(ItemRPC.RPC0, RPCA_Fire);
    }

    private void Update()
    {
        var amt = UnityInput.Current.GetKeyUp(KeyCode.UpArrow) ? 1f : UnityInput.Current.GetKeyUp(KeyCode.DownArrow) ? -1f : 0f;
        if (amt != 0f)
            if (UnityInput.Current.GetKey(KeyCode.LeftShift))
                itemInstance.item.alternativeHoldPos.y += Time.deltaTime * 10f * amt;
            else if (UnityInput.Current.GetKey(KeyCode.LeftControl))
                itemInstance.item.alternativeHoldPos.z += Time.deltaTime * 10f * amt;
            else
                itemInstance.item.alternativeHoldPos.x += Time.deltaTime * 10f * amt;
        
        UpdateCharges();

        HandleFlashlight();
        
        if (!isHeldByMe) return;
        
        if (timer > 0f)
            timer -= Time.deltaTime;
        
        TryShoot();
        TryHandleZoom();
    }

    private bool isControlledByMe => isHeldByMe || (!isHeld && PhotonNetwork.IsMasterClient); 
    private void HandleFlashlight()
    {
        if (isHeldByMe && Player.localPlayer.input.interactWasPressed && data.FlashlightBattery > 0f)
        {
            data.FlashlightOn = !data.FlashlightOn;
            data.SetDirty();
        }

        if (Flashlight.gameObject.activeSelf != data.FlashlightOn || FlashlightSignaler.gameObject.activeSelf != data.FlashlightOn)
        {
            Flashlight.gameObject.SetActive(data.FlashlightOn);
            FlashlightSignaler.gameObject.SetActive(data.FlashlightOn);
        }

        if (isControlledByMe && data.FlashlightOn)
        {
            data.FlashlightBattery = math.saturate(data.FlashlightBattery - (Time.deltaTime / FlashlightDepletionTime));
            
            if (data.FlashlightBattery == 0f)
                data.FlashlightOn = false;
            data.SetDirty();
        }
        if (isControlledByMe && !data.FlashlightOn && data.FlashlightBattery < 1f)
        {
            data.FlashlightBattery = math.saturate(data.FlashlightBattery + (Time.deltaTime / FlashlightRegenTime));
            data.SetDirty();
        }
        if (!Mathf.Approximately(lastFlashlightCharge, data.FlashlightBattery))
        {
            FlashlightBattery.SetCharge(data.FlashlightBattery);
            lastFlashlightCharge = data.FlashlightBattery;
        }
    }

    private void TryHandleZoom()
    {
        if (GlobalInputHandler.CanTakeInput() && Player.localPlayer.input.aimIsPressed)
            cameraZoom += Input.GetAxis("Mouse ScrollWheel");
        
        cameraZoom = math.saturate(cameraZoom);
        
        var desiredFov = CameraFOV / ((Player.localPlayer.input.aimIsPressed ? cameraZoom : 0f) * 5 + 1);
        if (!Mathf.Approximately(Camera.fieldOfView, desiredFov))
            Camera.fieldOfView = Mathf.MoveTowards(Camera.fieldOfView, desiredFov, 100 * Time.deltaTime);
    }

    private void EnableCameraView(bool enable = true)
    {
        if (Camera.targetTexture == null)
        {
            var renderTexture = new RenderTexture(420, 420, 24, GraphicsFormat.R8G8B8A8_UNorm);
            renderTexture.Create();
            Camera.targetTexture = renderTexture;
            CameraScreen.material.SetTexture(VideoCamera.BaseMap, renderTexture);
        }
        Camera.gameObject.SetActive(enable);
        CameraScreen.gameObject.SetActive(Camera is { enabled: true, gameObject.activeSelf: true });
    }
    
    private void TryShoot()
    {
        if (!Player.localPlayer.input.clickIsPressed || timer > 0f || data.Charge <= 0) return;

        timer = 1f / fireRate;
        
        data.ShotsFired++;
        data.SetDirty();
        
        var serializer = new BinarySerializer();
        serializer.WriteFloat3(BarrelTip.position);
        serializer.WriteFloat3(BarrelTip.forward);
        itemInstance.CallRPC(ItemRPC.RPC0, serializer);
    }
    private void RPCA_Fire(BinaryDeserializer deserializer)
    {
        var pos = (Vector3)deserializer.ReadFloat3();
        var rot = (Vector3)deserializer.ReadFloat3();
        Fire(pos, rot);
    }

    private void UpdateCharges()
    {
        var isShooting = isHeldByMe && Player.localPlayer.input.clickIsPressed && data.Charge != 0;
        
        if (isShooting) // Deplete if we're shooting
            chargeRegenTimer -= ChargeDepletionTime * Time.deltaTime;
        else if (isControlledByMe)
            chargeRegenTimer += ChargeRegenTime * Time.deltaTime;
        
        if (chargeRegenTimer is >= 1f or <= -1f && isControlledByMe)
        {
            data.Charge = Math.Clamp(data.Charge + (chargeRegenTimer >= 0f ? 1 : -1), 0, Charges.Length);
            data.SetDirty();
            chargeRegenTimer = 0f;
        }
        if (data.Charge == lastCharge) return;
        
        ChargeSFX.Play();
        lastCharge = data.Charge;
        for (var idx = 0; idx < Charges.Length; idx++)
        {
            Charges[idx].gameObject.SetActive(idx < lastCharge);
        }
    }

    private void Fire(Vector3 position, Vector3 direction, float velocity = 100)
    {
        var projectile = Instantiate(BulletProjectile, position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
        AccessTools.Method(typeof(Projectile), nameof(Projectile.Ignore))
            .Invoke(projectile, [ BarrelTip.root, 1f ]);
        projectile.velocity = velocity;
        projectile.damage = 0;
        projectile.force = 0;
        projectile.fall = 0f;
        projectile.GetComponent<AddGamefeel>().perlinAmount = 2;
        projectile.GetComponent<AddGamefeel>().range = 20;

        projectile.hitAction += hit =>
        {
            projectile.GetComponent<AddGamefeel>().AddPerlin();
            if (!isHeldByMe) return;
            var player = hit.collider.GetComponentInParent<Player>();
            if (player == null || !player) return;
            player.CallTakeDamageAndAddForceAndFallWithFallof(Random.Range(10, 25), projectile.vel / (player.ai ? 0.1f : 10f), 0.1f, hit.point, 1f);
        };
    }
}