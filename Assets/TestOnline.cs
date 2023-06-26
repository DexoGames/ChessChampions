using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TestOnline : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        TestServerRpc();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("e");
            TestServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void TestServerRpc()
    {
        Debug.Log("This isn't being printed AAAAAAAA");
    }
}
