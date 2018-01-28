﻿using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.Celeste {
	//.load C:\Windows\Microsoft.NET\Framework\v4.0.30319\SOS.dll
	public partial class SplitterMemory {
		private static ProgramPointer Celeste = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "83C604F30F7E06660FD6078BCBFF15????????8D15", 21));
		//private static ProgramPointer AreaData = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "8B3D????????3B770C720DB90D0000008D5109E8????????8B47043B70040F83", 2));
		private static ProgramPointer SaveData = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "8B7C90088B432C8B40048D5B6C8B53043B500473368B7490088B15", 27));
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SplitterMemory() {
			lastHooked = DateTime.MinValue;
		}
		public string RAMPointers() {
			return Celeste.GetPointer(Program).ToString("X") + " " + SaveData.GetPointer(Program).ToString("X");
		}
		public void DebugMenu(bool enabled) {
			//Celeste.Commands.Enabled
			Celeste.Write<bool>(Program, enabled, 0x8, 0x30);
		}
		public bool DebugMenuEnabled() {
			//Celeste.Commands.Enabled
			return Celeste.Read<bool>(Program, 0x8, 0x30);
		}
		public bool LevelCompleted() {
			//Celeste.scene.MethodTable.TypeSize
			int size = Celeste.Read<int>(Program, 0x0, 0x98, 0x0, 0x4);
			if (size == 340) {
				//((Level)Celeste.scene).Completed
				return Celeste.Read<bool>(Program, 0x0, 0x98, 0x2a);
			}
			return false;
		}
		public bool ShowInputUI() {
			//Celeste.scene.MethodTable.TypeSize
			int size = Celeste.Read<int>(Program, 0x0, 0x98, 0x0, 0x4);
			if (size == 100) {
				//((Overworld)Celeste.scene).showInputUI
				return Celeste.Read<bool>(Program, 0x0, 0x98, 0x2b);
			}
			return false;
		}
		public Menu MenuType() {
			//Celeste.scene.MethodTable.TypeSize
			int size = Celeste.Read<int>(Program, 0x0, 0x98, 0x0, 0x4);
			if (size == 100) {
				//((Overworld)Celeste.scene).current.MethodTable.TypeSize
				return (Menu)Celeste.Read<int>(Program, 0x0, 0x98, 0x30, 0x0, 0x4);
			}
			return Menu.InGame;
		}
		public double GameTime() {
			//SaveData.Instance.Time
			return (double)SaveData.Read<long>(Program, 0x0, 0x4) / (double)10000000;
		}
		public int Strawberries() {
			//SaveData.Instance.TotalStrawberries
			return SaveData.Read<int>(Program, 0x0, 0x34);
		}
		public int Deaths() {
			//SaveData.Instance.TotalDeaths
			return SaveData.Read<int>(Program, 0x0, 0x30);
		}
		public double LevelTime() {
			//SaveData.Instance.CurrentSession.Time
			return (double)SaveData.Read<long>(Program, 0x0, 0x24, 0x4) / (double)10000000;
		}
		public Area AreaID() {
			//SaveData.Instance.CurrentSession.Area.ID
			return (Area)SaveData.Read<int>(Program, 0x0, 0x24, 0x6c);
		}
		public AreaMode AreaDifficulty() {
			//SaveData.Instance.CurrentSession.Area.ID
			return (AreaMode)SaveData.Read<int>(Program, 0x0, 0x24, 0x70);
		}
		public int Cassettes() {
			//SaveData.Instance.Areas.Size
			int size = SaveData.Read<int>(Program, 0x0, 0x28, 0xc);
			int count = 0;
			for (int i = 0; i < size; i++) {
				//SaveData.Instance.Areas[i].Cassette
				if (SaveData.Read<bool>(Program, 0x0, 0x28, 0x4, 0x8 + (i * 4), 0xc)) {
					count++;
				}
			}
			return count;
		}
		public int HeartGems() {
			//SaveData.Instance.Areas.Size
			int size = SaveData.Read<int>(Program, 0x0, 0x28, 0xc);
			int count = 0;
			for (int i = 0; i < size; i++) {
				IntPtr area = (IntPtr)SaveData.Read<uint>(Program, 0x0, 0x28, 0x4, 0x8 + (i * 4), 0x4);
				for (int j = 0; j < 3; j++) {
					//SaveData.Instance.Areas[i].Modes[j].HeartGem
					if (Program.Read<bool>(area, 0x8 + (j * 4), 0x36)) {
						count++;
					}
				}
			}
			return count;
		}
		public string LevelName() {
			//SaveData.Instance.CurrentSession.Level
			return SaveData.Read(Program, 0x0, 0x24, 0x34, 0x0);
		}
		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Celeste");
				Program = processes != null && processes.Length > 0 ? processes[0] : null;

				if (Program != null && !Program.HasExited) {
					MemoryReader.Update64Bit(Program);
					IsHooked = true;
				}
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
}