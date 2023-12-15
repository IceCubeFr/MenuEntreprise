using System;
using System.Collections.Generic;
using Life;
using Life.BizSystem;
using Life.DB;
using Life.Network;
using Life.UI;
using UnityEngine;
using MyMenu.Entities;

namespace Menu_entreprise
{
    public class Main:Plugin // Classe de type plugin
    {
        public Main(IGameAPI api):base(api) // Constructeur
        {
            
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            // MyMenu

            Section section = new Section(Section.GetSourceName(), Section.GetSourceName(), "v0.0.1", "IceCubeFr");
            Action<UIPanel> action = ui => menuPrincipal(section.GetPlayer(ui));
            section.SetBizIdAllowed();
            section.SetBizTypeAllowed();
            section.OnlyAdmin = false;
            section.MinAdminLevel = 0;
            section.Line = new UITabLine(section.Title, action);
            section.Insert();
            Debug.Log("Le plugin Menu Entreprise s'est lance avec succes !");
        }

        // DateTime GetDateTime()

        // Définition des variables réutilisées

        string colorOpen = "<color=#52FF33>";
        string colorClose = "<color=#FF3333>";
        string color = "<color=#F4D03F>";
        string entete = "<color=#FF5733>[Entreprise] ";
        Dictionary<string, DateTime> openCool = new Dictionary<string, DateTime>();
        Dictionary<string, DateTime> recrutCool = new Dictionary<string, DateTime>();

        // Fonctions de base

        void openCooldown(UIPanel panel, int minutes, Player player)
        {
            DateTime now = DateTime.Now;
            DateTime cooldownEnd;
            string playerName = player.netId.ToString();
            if (openCool.TryGetValue(playerName, out cooldownEnd) && now.CompareTo(cooldownEnd) < 0)
            {
                player.Notify("Erreur", $"Cooldown en cours. Réessayez à {cooldownEnd.Hour}h{cooldownEnd.Minute}.", NotificationManager.Type.Error);
            }
            else if (openCool.TryGetValue(playerName, out cooldownEnd))
            {
                Validation(panel, player);
                openCool[playerName] = now.AddMinutes(minutes);
            }
            else
            {
                Validation(panel, player);
                openCool.Add(playerName, now.AddMinutes(minutes));
            }
        }



        void recrutCooldown(UIPanel panel, int minutes, Player player)
        {
            DateTime now = DateTime.Now;
            DateTime cooldownEnd;
            string playerName = player.netId.ToString();
            if (recrutCool.TryGetValue(playerName, out cooldownEnd) && now.CompareTo(cooldownEnd) < 0)
            {
                player.Notify("Erreur", $"Cooldown en cours. Réessayez à {cooldownEnd.Hour}h{cooldownEnd.Minute}.", NotificationManager.Type.Error);
            }
            else if (recrutCool.TryGetValue(playerName, out cooldownEnd))
            {
                Validation(panel, player);
                recrutCool[playerName] = now.AddMinutes(minutes);
            }
            else
            {
                Validation(panel, player);
                recrutCool.Add(playerName, now.AddMinutes(minutes));
            }
        }

        void Validation(UIPanel panel, Player player)
        {
            player.ClosePanel(panel);
            panel.SelectTab();
        }

        void openMessage(string entreprise, bool isOpen)
        {
            Nova.server.SendMessageToAll($"{entete}{color}L'entreprise {entreprise} est désormais {(isOpen ? $"{colorOpen}ouverte." : $"{colorClose}fermée.</color>")}");
        }

        void Retour(UIPanel panel, Player player)
        {
            player.ClosePanel(panel);
            menuPrincipal(player);
        }

        void recrutMessage(string entreprise, int type)
        {
            string start = $"{entete}{colorOpen}{entreprise} recrute";
            if (type == 0)
            {
                Nova.server.SendMessageToAll($"{start} activement");
            }
            else if (type == 1)
            {
                Nova.server.SendMessageToAll($"{start} exceptionnellement! Ne perdez pas de temps!");
            }
            else
            {
                Nova.server.SendMessageToAll($"{start} peu de nouvelles recrues! Saisissez votre chance rapidement.");
            }
        }

        void menuOuverture(Player player)
        {
            Biz biz = player.biz.GetBiz();
            UIPanel panelOuverture = new UIPanel("Menu Ouverture", UIPanel.PanelType.Tab);
            panelOuverture.AddButton("Annuler", ui => player.ClosePanel(panelOuverture));
            panelOuverture.AddButton("Valider", ui => openCooldown(panelOuverture, 10, player));
            panelOuverture.AddButton("Retour", ui => Retour(panelOuverture, player));
            panelOuverture.AddTabLine("Ouvrir votre entreprise", ui => openMessage(biz.bizName, true));
            panelOuverture.AddTabLine("Fermer votre entreprise", ui => openMessage(biz.bizName, false));
            player.ShowPanelUI(panelOuverture);
        }

        void menuRecrutement(Player player)
        {
            Biz biz = player.biz.GetBiz();
            UIPanel panelRecrut = new UIPanel("Menu Recrutements", UIPanel.PanelType.Tab);
            panelRecrut.AddButton("Annuler", ui => player.ClosePanel(panelRecrut));
            panelRecrut.AddButton("Valider", ui => recrutCooldown(panelRecrut, 5, player));
            panelRecrut.AddButton("Retour", ui => Retour(panelRecrut, player));
            panelRecrut.AddTabLine("Recrutements actifs", ui => recrutMessage(biz.bizName, 0));
            panelRecrut.AddTabLine("Recrutements exceptionnels", ui => recrutMessage(biz.bizName, 1));
            panelRecrut.AddTabLine("Recrutements limités", ui => recrutMessage(biz.bizName, 2));
            player.ShowPanelUI(panelRecrut);
        }

        // Code principal

        void menuPrincipal(Player player)
        {
            if (player.HasBiz())
            {
                Biz biz = player.biz.GetBiz();
                UIPanel panel = new UIPanel("Menu Entreprise", UIPanel.PanelType.Tab);
                panel.AddButton("Annuler", ui => player.ClosePanel(panel));
                panel.AddButton("Valider", ui => Validation(panel, player));
                panel.AddTabLine("Ouverture / Fermeture", ui => menuOuverture(player));
                panel.AddTabLine("Recrutements", ui => menuRecrutement(player));
                player.ShowPanelUI(panel);
            }

            else
            {
                player.Notify("ERREUR", "Vous n'êtes pas dans une entreprise", NotificationManager.Type.Error);
            }
        }
    }
}
