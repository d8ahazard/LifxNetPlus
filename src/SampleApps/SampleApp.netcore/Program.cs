using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LifxNetPlus;
using Newtonsoft.Json;

namespace SampleApp.netcore {
	/// <summary>
	/// The program class
	/// </summary>
	class Program {
		/// <summary>
		/// The client
		/// </summary>
		static LifxClient _client;

		/// <summary>
		/// Main
		/// </summary>
		static void Main() {
			var tr1 = new TextWriterTraceListener(Console.Out);
			Trace.Listeners.Add(tr1);
			_client = LifxClient.CreateAsync().Result;
			_client.DeviceDiscovered += ClientDeviceDiscovered;
			_client.DeviceLost += ClientDeviceLost;
			_client.StartDeviceDiscovery();
			Console.ReadKey();
		}

		/// <summary>
		/// Clients the device lost using the specified sender
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The </param>
		private static void ClientDeviceLost(object sender, LifxClient.DeviceDiscoveryEventArgs e) {
			Console.WriteLine("Device lost");
		}

		/// <summary>
		/// Clients the device discovered using the specified sender
		/// </summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The </param>
		private static async void ClientDeviceDiscovered(object sender, LifxClient.DeviceDiscoveryEventArgs e) {
			Console.WriteLine($"Device {e.Device.MacAddressName} found @ {e.Device.HostName}");

			var pwr = _client.SetLightPowerAsync(e.Device,true);
			
			var foo2 = await _client.GetWanAsync(e.Device);
			Console.WriteLine("Result of 201: " + JsonConvert.SerializeObject(foo2));


			var foo1 = await _client.GetDeviceOwnerAsync(e.Device);
			//Console.WriteLine("Owner: " + JsonConvert.SerializeObject(foo1));
			var pstring = foo1.Payload.ToArray().Select(b => "0x" + b.ToString("X2")).ToList();
			Console.WriteLine("Owner: " +
			                  string.Join(',', pstring));
			
			

			var wifi = await _client.GetWifiFirmwareAsync(e.Device);
			Console.WriteLine("Wifi info: " + JsonConvert.SerializeObject(wifi));

			var version = await _client.GetDeviceVersionAsync(e.Device);
			Console.WriteLine("Version info: " + JsonConvert.SerializeObject(version));

			// Multi-zone devices
			if (version.Product == 31 || version.Product == 32 || version.Product == 38) {
				Console.WriteLine("Device is multi-zone, enumerating data.");
				var extended = false;
				// If new Z-LED or Beam, check if FW supports "extended" commands.
				if (version.Product == 32 || version.Product == 38) {
					if (version.Version >= 1532997580) {
						extended = true;
						Console.WriteLine("Enabling extended firmware features.");
					}
				}

				if (extended) {
					var zones = await _client.GetExtendedColorZonesAsync(e.Device);
					Console.WriteLine("Zones: " + JsonConvert.SerializeObject(zones));
				} else {
					// Original device only supports eight zones?
					var zones = await _client.GetColorZonesAsync(e.Device, 0, 8);
					Console.WriteLine("Zones: " + JsonConvert.SerializeObject(zones));
				}
			}

			// Tile
			if (version.Product == 55) {
				Console.WriteLine("Device is a tile group, enumerating data.");
				var chain = await _client.GetDeviceChainAsync(e.Device);
				Console.WriteLine("Tile chain: " + JsonConvert.SerializeObject(chain));
			}

			// Switch
			if (version.Product == 70) {
				Console.WriteLine("Device is a switch, enumerating data.");
				var switchState = await _client.GetRelayPowerAsync(e.Device, 0);
				Console.WriteLine($"Switch State: {switchState.Level}");
			}
		}
	}
}