using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{
    private NetworkManager networkManager;

    void Start()
    {
        networkManager = GetComponentInParent<NetworkManager>();

        if (Application.isEditor)
        {
            return;
        }

        Dictionary<string, string> args = GetCommandlineArgs();

        if (args.TryGetValue("-mlapi", out string mlapiValue))
        {
            switch (mlapiValue)
            {
                case "server":
                    networkManager.StartServer();

                    break;
                case "host":
                    networkManager.StartHost();

                    break;
                case "client":
                    networkManager.StartClient();

                    break;
            }
        }
    }

    private Dictionary<string, string> GetCommandlineArgs()
    {
        var argDictionary = new Dictionary<string, string>();
        string[] args = Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();

            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;
                argDictionary.Add(arg, value);
            }
        }

        return argDictionary;
    }
}
