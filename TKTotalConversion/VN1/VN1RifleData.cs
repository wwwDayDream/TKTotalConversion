using Unity.Mathematics;
using UnityEngine;
using Zorro.Core.Serizalization;

namespace TKTotalConversion.VN1;

public class VN1RifleData(int startCharge = 5, bool flashlightOnEnabled = false) : ItemDataEntry {
    public const byte Identifier = 239;
    public int Charge { get; set; } = startCharge;
    public int ShotsFired { get; set; } = 0;
    public bool FlashlightOn { get; set; } = flashlightOnEnabled;
    public float FlashlightBattery { get; set; } = 1f;
    
    public override void Serialize(BinarySerializer binarySerializer)
    {
        binarySerializer.WriteInt(Charge);
        binarySerializer.WriteInt(ShotsFired);
        binarySerializer.WriteBool(FlashlightOn);
        binarySerializer.WriteFloat(FlashlightBattery);
    }

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        Charge = binaryDeserializer.ReadInt();
        ShotsFired = binaryDeserializer.ReadInt();
        FlashlightOn = binaryDeserializer.ReadBool();
        FlashlightBattery = binaryDeserializer.ReadFloat();
    }
}