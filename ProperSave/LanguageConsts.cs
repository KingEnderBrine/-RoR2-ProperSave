using RoR2;

namespace ProperSave
{
    internal class LanguageConsts
    {
        public const string PROPER_SAVE_TITLE_CONTINUE_DESC = nameof(PROPER_SAVE_TITLE_CONTINUE_DESC);
        public const string PROPER_SAVE_TITLE_CONTINUE = nameof(PROPER_SAVE_TITLE_CONTINUE);
        public const string PROPER_SAVE_TITLE_LOAD = nameof(PROPER_SAVE_TITLE_LOAD);
        public const string PROPER_SAVE_CHAT_SAVE = nameof(PROPER_SAVE_CHAT_SAVE);
        public const string PROPER_SAVE_QUIT_DIALOG_SAVED = nameof(PROPER_SAVE_QUIT_DIALOG_SAVED);
        public const string PROPER_SAVE_QUIT_DIALOG_SAVED_BEFORE = nameof(PROPER_SAVE_QUIT_DIALOG_SAVED_BEFORE);
        public const string PROPER_SAVE_QUIT_DIALOG_NOT_SAVED = nameof(PROPER_SAVE_QUIT_DIALOG_NOT_SAVED);
        public const string PROPER_SAVE_TOOLTIP_LOAD_TITLE = nameof(PROPER_SAVE_TOOLTIP_LOAD_TITLE);
        public const string PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_BODY = nameof(PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_BODY);
        public const string PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_CHARACTER = nameof(PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_CHARACTER);

        public static void OnLoadStrings(On.RoR2.Language.orig_LoadStrings orig, Language self)
        {
            orig(self);

            switch(self.name.ToLower())
            {
                case "ru":
                    self.SetStringByToken(PROPER_SAVE_TITLE_CONTINUE_DESC, "Продолжить сохранённую игру");
                    self.SetStringByToken(PROPER_SAVE_TITLE_CONTINUE, "Продолжить");
                    self.SetStringByToken(PROPER_SAVE_TITLE_LOAD, "Загрузить");
                    self.SetStringByToken(PROPER_SAVE_CHAT_SAVE, "<b>[ProperSave]</b> Игра сохранена (Этап: {0})");
                    self.SetStringByToken(PROPER_SAVE_QUIT_DIALOG_SAVED, "\n\n<b>[ProperSave]</b> Игра была сохранена в начале текущего этапа");
                    self.SetStringByToken(PROPER_SAVE_QUIT_DIALOG_SAVED_BEFORE, "\n\n<b>[ProperSave]</b> Игра была сохранена {0} этапов назад");
                    self.SetStringByToken(PROPER_SAVE_QUIT_DIALOG_NOT_SAVED, "\n\n<b>[ProperSave]</b> Игра пока не была сохранена");
                    self.SetStringByToken(PROPER_SAVE_TOOLTIP_LOAD_TITLE, "Информация о сохранении");
                    self.SetStringByToken(PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_BODY, "Персонажи:\n{0}Этап:\n  Название: <color=#ffff00>{1}</color>\n  Номер: <color=#ffff00>{2}</color>\nВремя: <color=#ffff00>{3}</color>\nСложность: <color=#ffff00>{4}</color>");
                    self.SetStringByToken(PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_CHARACTER, "  {0}: <color=#ffff00>{1}</color>\n");
                    break;
                default:
                    self.SetStringByToken(PROPER_SAVE_TITLE_CONTINUE_DESC, "Continue saved game");
                    self.SetStringByToken(PROPER_SAVE_TITLE_CONTINUE, "Continue");
                    self.SetStringByToken(PROPER_SAVE_TITLE_LOAD, "Load");
                    self.SetStringByToken(PROPER_SAVE_CHAT_SAVE, "<b>[ProperSave]</b> The game saved (Stage: {0})");
                    self.SetStringByToken(PROPER_SAVE_QUIT_DIALOG_SAVED, "\n\n<b>[ProperSave]</b> The game was saved at the start of the current stage");
                    self.SetStringByToken(PROPER_SAVE_QUIT_DIALOG_SAVED_BEFORE, "\n\n<b>[ProperSave]</b> The game was saved {0} stage(s) ago");
                    self.SetStringByToken(PROPER_SAVE_QUIT_DIALOG_NOT_SAVED, "\n\n<b>[ProperSave]</b> The game was not saved yet");
                    self.SetStringByToken(PROPER_SAVE_TOOLTIP_LOAD_TITLE, "Save info");
                    self.SetStringByToken(PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_BODY, "Characters:\n{0}Stage:\n  Name: <color=#ffff00>{1}</color>\n  Number: <color=#ffff00>{2}</color>\nTime: <color=#ffff00>{3}</color>\nDifficulty: <color=#ffff00>{4}</color>");
                    self.SetStringByToken(PROPER_SAVE_TOOLTIP_LOAD_DESCRIPTION_CHARACTER, "  {0}: <color=#ffff00>{1}</color>\n");
                    break;
            }
        }
    }
}
