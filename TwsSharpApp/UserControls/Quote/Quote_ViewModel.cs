﻿using IBApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TwsSharpApp.Data;

namespace TwsSharpApp
{
    public class Quote_ViewModel : Workspace_ViewModel
    {
        public string Symbol { get { return ContractDetails.Symbol; } }
        public int ReqId { get; set; }

        public ContractDetails_ViewModel ContractDetails { get; set; }

        public Quote_ViewModel(int reqId, ContractDetails_ViewModel cDetails)
        {
            ReqId  = reqId;
            ContractDetails = cDetails;
            IsTabSelected = true;
        }

        public void SetClosedValues(List<Bar> closePricesList)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;

            DateTime timeClosed = DateTime.Now;

            if (closePricesList.Count == 1)
            {
                PrevClose   = closePricesList[0].Open;
                LowValue    = closePricesList[0].Low;
                HighValue   = closePricesList[0].High;
                LatestClose = closePricesList[0].Close;

                timeClosed = DateTime.ParseExact(closePricesList[0].Time, "yyyyMMdd", provider);
            }
            else if (closePricesList.Count == 2)
            {
                PrevClose   = closePricesList[0].Close;
                LowValue    = closePricesList[1].Low;
                HighValue   = closePricesList[1].High;
                LatestClose = closePricesList[1].Close;

                timeClosed = DateTime.ParseExact(closePricesList[1].Time, "yyyyMMdd", provider);
            }

            Time = timeClosed.ToShortDateString();
            prevRealTime = timeClosed;
        }

        DateTime prevRealTime;

        public void UpdateRealTimeData(Bar realtimeBar)
        {
            DateTime timeBar = DateTime.Parse(realtimeBar.Time);

            if(prevRealTime <= ContractDetails.OpeningTime && timeBar > ContractDetails.OpeningTime)
            {
                Debug.WriteLine($"UpdateRealTimeData: {Symbol}, {lowValue}, {highValue}, {prevRealTime},  {timeBar}");
                highValue = 0;
                lowValue = double.MaxValue;
                prevRealTime = timeBar;
            }

            LowValue  = realtimeBar.Low;
            HighValue = realtimeBar.High;
            Latest    = realtimeBar.Close;
            Time      = timeBar.ToString("HH:mm:ss");
        }

        public void RenewPreviousClose()
        {
            //Debug.WriteLine("RenewPreviousClose " + DateTime.Now.ToString("h:mm:ss.fff") + " " + Symbol);

            PrevClose = LatestClose;
            IsDefined = false;

            updateVariations();
        }

        public string UniqueName { get { return ContractDetails.UniqueName; } }

        private double prevClose = 0;
        public  double PrevClose
        {
            get { return prevClose; }
            set 
            {
                if (prevClose == value) return;
                prevClose = value;
                OnPropertyChanged(nameof(PrevClose));
            }
        }

        private double latestClose = 0;
        public  double LatestClose
        {
            get { return latestClose; }
            set 
            {
                if (latestClose == value) return;
                latestClose = value;
                OnPropertyChanged(nameof(LatestClose));

                Latest = latestClose;
            }
        }

        private double lowValue = double.MaxValue;
        public  double LowValue
        {
            get
            {
                if (lowValue == double.MaxValue) return 0;

                return lowValue;
            }

            set
            {
                if (lowValue <= value) return;
                lowValue = value;
                OnPropertyChanged(nameof(LowValue));
            }
        }

        private double highValue = 0;
        public  double HighValue
        {
            get { return highValue; }
            set
            {
                if (highValue >= value) return;
                highValue = value;
                OnPropertyChanged(nameof(HighValue));
            }
        }

        private double previous = 0;
        private double latest = double.NaN;
        public  double Latest
        {
            get { return latest; }
            set
            {
                if (latest == value) return;
                previous = latest;
                latest = value;
                OnPropertyChanged(nameof(Latest));

                updateVariations();
            }
        }

        private void updateVariations()
        {
            IsDefined = !(double.IsNaN(latest));
                
            // Compute variations:
            Var = latest - prevClose;
            VarPercent = 100 * var / prevClose;

            OnPropertyChanged(nameof(Background_TickVariation));
        }

        private bool isDefined = false;
        public  bool IsDefined
        {
            get { return isDefined; }
            set
            {
                if (isDefined == value) return;
                isDefined = value;
                OnPropertyChanged(nameof(IsDefined));
            }
        }

        private double var = 0;
        public  double Var
        {
            get { return var; }
            set
            {
                if (var == value) return;
                bool isColorChanging = (Math.Sign(var) != Math.Sign(value));

                var = value;

                if (isColorChanging)
                {
                    OnPropertyChanged(nameof(Background_DailyVariation));
                }
                OnPropertyChanged(nameof(Var));
            }
        }

        private double varPercent = 0;
        public  double VarPercent
        {
            get { return varPercent; }
            set
            {
                if (varPercent == value) return;
                varPercent = value;
                OnPropertyChanged(nameof(VarPercent));
            }
        }

        private string time;
        public  string Time
        {
            get { return time; }
            set 
            {
                if (time == value) return;
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        Color Color_Var_Daily_Backgroud_Positive = (Color)Application.Current.Resources["Color.Var.Daily.Backgroud.Positive"];
        Color Color_Var_Daily_Backgroud_Negative = (Color)Application.Current.Resources["Color.Var.Daily.Backgroud.Negative"];
        Color Color_Var_Daily_Backgroud_Zero     = (Color)Application.Current.Resources["Color.Var.Daily.Backgroud.Zero"];

        public Color Background_DailyVariation
        {
            get
            {
                if (Var < 0)
                    return Color_Var_Daily_Backgroud_Negative;
                else if (Var > 0)
                    return Color_Var_Daily_Backgroud_Positive;
                else
                    return Color_Var_Daily_Backgroud_Zero;
            }
        }

        Color Color_Var_Tick_Backgroud_Positive = (Color)Application.Current.Resources["Color.Var.Tick.Backgroud.Positive"];
        Color Color_Var_Tick_Backgroud_Negative = (Color)Application.Current.Resources["Color.Var.Tick.Backgroud.Negative"];
        Color Color_Var_Tick_Backgroud_Zero     = (Color)Application.Current.Resources["Color.Var.Tick.Backgroud.Zero"];

        public Color Background_TickVariation
        {
            get
            {
                if (latest - previous < 0)
                    return Color_Var_Tick_Backgroud_Negative;
                else if (latest - previous > 0)
                    return Color_Var_Tick_Backgroud_Positive;
                else
                    return Color_Var_Tick_Backgroud_Zero;
            }
        }

        public async void SaveToDB()
        {
            DB_ModelContainer db = new DB_ModelContainer();

            ContractData cData = db.DisplayedContracts.FirstOrDefault(c => c.Symbol  == ContractDetails.Contract.Symbol &&
                                                                           c.SecType == ContractDetails.Contract.SecType);

            if(cData == null)
            {
                ContractData cd = new ContractData(ContractDetails.Contract);

                db.DisplayedContracts.Add(cd);
                await db.SaveChangesAsync();
            }
        }

        private bool isMouseOver = false;
        public  bool IsMouseOver
        {
            get { return isMouseOver && !IsFullScreen; }
            set
            {
                if (isMouseOver == value) return;
                isMouseOver = value;
                OnPropertyChanged(nameof(IsMouseOver));
            }
        }

        private RelayCommand deleteFromDB_Command;
        public  RelayCommand DeleteFromDB_Command
        {
            get
            {
                return deleteFromDB_Command ?? (deleteFromDB_Command = new RelayCommand(async param => await this.DeleteFromDB(), param => true));
            }
        }

        public static event EventHandler ContractRemoved_Event;

        public async Task DeleteFromDB()
        {
            DB_ModelContainer db = new DB_ModelContainer();

            ContractData cData = db.DisplayedContracts.FirstOrDefault(c => c.Symbol  == ContractDetails.Contract.Symbol &&
                                                                           c.SecType == ContractDetails.Contract.SecType);

            if(cData != null)
            {
                db.DisplayedContracts.Remove(cData);
                await db.SaveChangesAsync();
                ContractRemoved_Event?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
