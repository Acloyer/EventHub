using System;
using System.Collections.Generic;

namespace EventHub.Services
{
    public class NotificationLocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations;

        public NotificationLocalizationService()
        {
            _localizations = new Dictionary<string, Dictionary<string, string>>
            {
                ["en"] = new Dictionary<string, string>
                {
                    ["eventReminder"] = "Reminder: event \"{0}\" starts at {1}",
                    ["eventStartsIn"] = "Event \"{0}\" starts in {1} minutes",
                    ["newReaction"] = "Someone reacted to your event \"{0}\"",
                    ["newComment"] = "Someone commented on your event \"{0}\"",
                    ["eventUpdated"] = "Event \"{0}\" was updated",
                    ["eventCancelled"] = "Event \"{0}\" was cancelled",
                    ["welcomeMessage"] = "Welcome to EventHub! You will receive notifications about your events here.",
                    ["newNotification"] = "New notification"
                },
                ["ru"] = new Dictionary<string, string>
                {
                    ["eventReminder"] = "Напоминание: событие \"{0}\" начинается в {1}",
                    ["eventStartsIn"] = "Событие \"{0}\" начинается через {1} минут",
                    ["newReaction"] = "Кто-то отреагировал на ваше событие \"{0}\"",
                    ["newComment"] = "Кто-то прокомментировал ваше событие \"{0}\"",
                    ["eventUpdated"] = "Событие \"{0}\" было обновлено",
                    ["eventCancelled"] = "Событие \"{0}\" было отменено",
                    ["welcomeMessage"] = "Добро пожаловать в EventHub! Здесь вы будете получать уведомления о ваших событиях.",
                    ["newNotification"] = "Новое уведомление"
                },
                ["az"] = new Dictionary<string, string>
                {
                    ["eventReminder"] = "Xatırlatma: \"{0}\" tədbiri {1}-də başlayır",
                    ["eventStartsIn"] = "\"{0}\" tədbiri {1} dəqiqə sonra başlayır",
                    ["newReaction"] = "Kimsə \"{0}\" tədbirinizə reaksiya verdi",
                    ["newComment"] = "Kimsə \"{0}\" tədbirinizə şərh yazdı",
                    ["eventUpdated"] = "\"{0}\" tədbiri yeniləndi",
                    ["eventCancelled"] = "\"{0}\" tədbiri ləğv edildi",
                    ["welcomeMessage"] = "EventHub-a xoş gəlmisiniz! Burada tədbirləriniz haqqında bildirişlər alacaqsınız.",
                    ["newNotification"] = "Yeni bildiriş"
                }
            };
        }

        public string GetLocalizedMessage(string language, string key, params object[] args)
        {
            // Если язык не поддерживается, используем английский
            if (!_localizations.ContainsKey(language))
            {
                language = "en";
            }

            if (!_localizations[language].ContainsKey(key))
            {
                // Если ключ не найден в выбранном языке, используем английский
                if (_localizations["en"].ContainsKey(key))
                {
                    return string.Format(_localizations["en"][key], args);
                }
                return key; // Возвращаем ключ как fallback
            }

            return string.Format(_localizations[language][key], args);
        }

        public string GetEventReminderMessage(string language, string eventTitle, DateTime startTime)
        {
            return GetLocalizedMessage(language, "eventReminder", eventTitle, startTime.ToString("HH:mm"));
        }

        public string GetEventStartsInMessage(string language, string eventTitle, int minutes)
        {
            return GetLocalizedMessage(language, "eventStartsIn", eventTitle, minutes);
        }

        public string GetNewReactionMessage(string language, string eventTitle)
        {
            return GetLocalizedMessage(language, "newReaction", eventTitle);
        }

        public string GetNewCommentMessage(string language, string eventTitle)
        {
            return GetLocalizedMessage(language, "newComment", eventTitle);
        }

        public string GetEventUpdatedMessage(string language, string eventTitle)
        {
            return GetLocalizedMessage(language, "eventUpdated", eventTitle);
        }

        public string GetEventCancelledMessage(string language, string eventTitle)
        {
            return GetLocalizedMessage(language, "eventCancelled", eventTitle);
        }

        public string GetWelcomeMessage(string language)
        {
            return GetLocalizedMessage(language, "welcomeMessage");
        }
    }
} 