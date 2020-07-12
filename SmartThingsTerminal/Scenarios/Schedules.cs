﻿using Newtonsoft.Json;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace SmartThingsTerminal.Scenarios
{
    [ScenarioMetadata(Name: "Schedules", Description: "SmartThings application schedules")]
    [ScenarioCategory("Schedules")]
    class Schedules : Scenario
    {
        public override void Setup()
        {
            Dictionary<string, dynamic> dataItemList = null;
            Dictionary<string, string> displayItemList = null;
            try
            {
                if (STClient.GetAllSchedules().Items?.Count > 0)
                {
                    dataItemList = STClient.GetAllSchedules().Items
                       .OrderBy(t => t.Name)
                       .Select(t => new KeyValuePair<string, dynamic>(t.Name, t))
                       .ToDictionary(t => t.Key, t => t.Value);

                    displayItemList = STClient.GetAllSchedules().Items
                        .OrderBy(o => o.Name)
                        .Select(t => new KeyValuePair<string, string>(t.Name, t.Name))
                        .ToDictionary(t => t.Key, t => t.Value);
                }
            }
            catch (SmartThingsNet.Client.ApiException exp)
            {
                ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
            }
            catch (Exception exp)
            {
                ShowErrorMessage($"Error {exp.Message}");
            }
            ConfigureWindows<Scenario>(displayItemList, dataItemList);
        }

        public override void ConfigureStatusBar()
        {
            StatusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => RefreshScreen()),
                new StatusItem(Key.F6, "~F6~ Copy Schedule", () => SaveItem(true)),
                new StatusItem(Key.F9, "~F9~ Delete Schedule", () => DeleteItem()),
                new StatusItem(Key.Home, "~Home~ Back", () => Quit())
            });
        }

        public override bool SaveItem(bool copyCurrent = false)
        {
            var json = JsonView?.Text.ToString();

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var schedule = JsonConvert.DeserializeObject<Schedule>(json);
                    var scheduleRequest = new ScheduleRequest(
                        cron: schedule.Cron,
                        name: schedule.Name);

                    scheduleRequest.Name = Guid.NewGuid().ToString();
                    STClient.CreateSchedule(schedule.InstalledAppId.ToString(), scheduleRequest);
                    RefreshScreen();
                }
                catch (SmartThingsNet.Client.ApiException exp)
                {
                    ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error {exp.Message}");
                }
            }
            return true;
        }

        public override void DeleteItem()
        {
            if (SelectedItem != null)
            {
                Schedule currentItem = (Schedule)SelectedItem;
                try
                {
                    STClient.DeleteSchedule(currentItem.InstalledAppId.ToString(), currentItem.Name);
                    base.DeleteItem();
                    RefreshScreen();
                }
                catch (SmartThingsNet.Client.ApiException exp)
                {
                    ShowErrorMessage($"Error {exp.ErrorCode}{Environment.NewLine}{exp.Message}");
                }
                catch (Exception exp)
                {
                    ShowErrorMessage($"Error {exp.Message}");
                }
            }
        }
    }
}
