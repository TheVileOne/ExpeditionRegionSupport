using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExpeditionRegionSupport
{
    public static class SaveUtils
    {
        public static bool MineForGameComplete(SlugcatStats.Name name)
        {
            var playerProgression = ProgressionData.PlayerData.ProgressData;

            if (!playerProgression.IsThereASavedGame(name))
                return false;

            SaveState currentSave = playerProgression.currentSaveState;

            if (currentSave != null && currentSave.saveStateNumber == name)
                return currentSave.deathPersistentSaveData.ascended || currentSave.deathPersistentSaveData.altEnding;

            string[] progLinesFromMemory = playerProgression.GetProgLinesFromMemory();
            if (progLinesFromMemory.Length == 0)
                return false;

            for (int i = 0; i < progLinesFromMemory.Length; i++)
            {
                string[] array = Regex.Split(progLinesFromMemory[i], "<progDivB>");
                if (array.Length == 2 && array[0] == "SAVE STATE" && array[1][21].ToString() == name.value)
                {
                    List<SaveStateMiner.Target> list = new List<SaveStateMiner.Target>();
                    list.Add(new SaveStateMiner.Target(">ASCENDED", null, "<dpA>", 20));
                    list.Add(new SaveStateMiner.Target(">ALTENDING", null, "<dpA>", 20));
                    List<SaveStateMiner.Result> list2 = SaveStateMiner.Mine(playerProgression.rainWorld, array[1], list);
                    bool flag = false;
                    bool flag2 = false;
                    for (int j = 0; j < list2.Count; j++)
                    {
                        string name_ = list2[j].name;
                        if (name_ == ">ASCENDED")
                        {
                            flag = true;
                        }
                        else if (name_ == ">ALTENDING")
                        {
                            flag2 = true;
                        }
                    }
                    return flag || flag2;
                }
            }
            return false;
        }
    }
}
