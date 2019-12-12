using System;
using System.Collections.Generic;
using System.Linq;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    class SettingsList
    {
        public enum Keys : int
        {
            ConnIp = 1,
            ConnPort,
            ConnClientID
        }

        public string[] DefaultValues = { "", "127.0.0.1", "7497", "1" };

        Dictionary<string, string> valuesList;
        Dictionary<string, string> preValuesList;

        public SettingsList()
        {
        }

        public string GetValueStr(Keys key)
        {
            return valuesList[key.ToString()];
        }

        public int? GetValueInt(Keys key)
        {
            int ret = 0;
            if (int.TryParse(valuesList[key.ToString()], out ret))
                return ret;
            else
                return null;
        }

        public void SetValue(Keys key, String value)
        {
            valuesList[key.ToString()] = value;
        }

        public void SetValue(Keys key, int? value)
        {
            if(value == null)
            {
                valuesList[key.ToString()] = DefaultValues[(int)key];
            }
            else
            {
                valuesList[key.ToString()] = value.ToString();
            }
        }

        public void RestoreValue(Keys key)
        {
            if(preValuesList[key.ToString()] != valuesList[key.ToString()])
            {
                valuesList[key.ToString()] = preValuesList[key.ToString()];
            }
        }

        public bool IsValueChanged(Keys key)
        {
            bool isChanged = false;

            if(preValuesList == null || 
               preValuesList.ContainsKey(key.ToString()) == false || 
               preValuesList[key.ToString()] != valuesList[key.ToString()])
            {
                isChanged = true;
            }

            return isChanged;
        }

        public void SaveToDB()
        {
            DB_ModelContainer db = new DB_ModelContainer();

            foreach(SettingsList.Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (IsValueChanged(key) == true)
                {
                    Setting set = db.Settings.FirstOrDefault(s => s.Key == key.ToString());
                    if(set == null)
                    {
                        set = new Setting(key.ToString(), valuesList[key.ToString()]);
                        db.Settings.Add(set);
                    }
                    else
                    {
                        set.Value = valuesList[key.ToString()];
                        db.Settings.Update(set);
                    }
                }
            }

            if(db.ChangeTracker.HasChanges() == true)
                db.SaveChanges();
        }

        public bool LoadSettingsFromDB()
        {
            bool settingsOK = true;
            DB_ModelContainer db = new DB_ModelContainer();

            valuesList = db.Settings.ToDictionary(s => s.Key, s => s.Value);

            preValuesList = new Dictionary<string, string>(valuesList);

            foreach(SettingsList.Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (valuesList.Keys.Contains(key.ToString()) == false)
                {
                    settingsOK = false;
                    valuesList[key.ToString()] = DefaultValues[(int)key];
                }
            }

            return settingsOK;
        }

        public void ConnUndo()
        {
            for(Keys i=Keys.ConnIp; i <= Keys.ConnClientID; i++)
            {
                RestoreValue(i);
            }
        }
    }
}
