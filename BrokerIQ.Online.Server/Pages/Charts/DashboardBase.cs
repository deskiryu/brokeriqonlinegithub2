
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrokerIQ.Online.Pages.Samples.Shared;
using BrokerIQ.Online.Services.Interface;
using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.LineChart;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;

namespace BrokerIQ.Online.Pages
{
    public class DashboardBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IChartDataService ChartDataService { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public IBrokerService BrokerService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

        [Parameter]
        public string BrokerId { get; set; }

        public LineConfig _lineConfig;
        public Chart _lineChartJs;

        public PieConfig _pieConfig;
        public Chart _pieChartJs;

        public PieConfig _pieConfig2;
        public Chart _pieChartJs2;

        public LineConfig _steppedConfig;
        public Chart _steppedChartJs;

        public BarConfig _barConfig;
        public Chart _barChartJs;

        private int _brokerId;


        private LineDataset<int> _SentDataSet;
        private LineDataset<int> _ConvertedDataSet;
        private PieDataset<int> _PieDataSet;
        private PieDataset<int> _PieDataSet2;
        private LineDataset<int> _SentSteppedDataSet;

        protected string Message = string.Empty;
        protected string StatusClass = string.Empty;
        protected bool Saved;

        public string BrokerName { get; set; }
        public int CustomerCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            // Charting
            _lineConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Invite emails sent to App downloads",
                        FontSize = 20
                    },
                    Scales = new Scales
                    {
                        XAxes = new List<CartesianAxis>
{
                        new CategoryAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Month"
                            }
                        }
                    },
                        YAxes = new List<CartesianAxis>
{
                        new LinearCartesianAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Value"
                            }
                        }
                    }
                    }
                }
            };

            _pieConfig = new PieConfig
            {
                Options = new PieOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Sign ups (Converted from invite/ chose broker from list)",
                        FontSize = 20
                    }
                }
            };

            _pieConfig2 = new PieConfig
            {
                Options = new PieOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Signed up after invite/ not signed up yet",
                        FontSize = 20
                    }
                }
            };

            _steppedConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "No of users/ no times logged in to app",
                        FontSize = 20
                    },
                    Scales = new Scales
                    {
                        XAxes = new List<CartesianAxis>
{
                        new CategoryAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Login amounts"
                            }
                            ,GridLines = new GridLines
                            {
                                OffsetGridLines=true
                            }
                        }

                    },
                        YAxes = new List<CartesianAxis>
{
                        new LinearCartesianAxis
                        {
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Users"
                            },


                        }
                    }
                    }
                }
            };

            // Charting
            _barConfig = new BarConfig()
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Position = ChartJs.Blazor.Common.Enums.Position.Top
                    },
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Invites to conversions",
                        FontSize = 20
                    }
                }
            };

            _SentDataSet = new LineDataset<int>
            {
                BackgroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.FromArgb(0x58, 0x58, 0x58)),
                BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Black),
                Label = "Invites sent per day",
                Fill = true,
                BorderWidth = 2,
                PointRadius = 2,
                PointBorderWidth = 2,
                SteppedLine = SteppedLine.False
            };

            _ConvertedDataSet = new LineDataset<int>
            {
                BackgroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.FromArgb(0xff, 0xfd, 0x7e)),
                BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.White),
                Label = "App downloads per day",
                Fill = true,
                BorderWidth = 2,
                PointRadius = 2,
                PointBorderWidth = 2,
                SteppedLine = SteppedLine.False
            };

            _SentSteppedDataSet = new LineDataset<int>
            {
                BackgroundColor = ColorUtil.FromDrawingColor(System.Drawing.Color.FromArgb(0x58, 0x58, 0x58)),
                BorderColor = ColorUtil.FromDrawingColor(System.Drawing.Color.Black),
                Label = "Logins",
                Fill = false,
                BorderWidth = 2,
                PointRadius = 2,
                PointBorderWidth = 2,
                SteppedLine = SteppedLine.True
            };

            try
            {
                var user = await AccountService.GetUser();

                if (user is not null)
                {
                    if (user.IsBroker)
                    {
                        _brokerId = user.MasterBrokerId;
                    }

                    if (user.IsAdmin || user.IsMinorAdmin)
                    {
                        _brokerId = 0;
                        Int32.TryParse(BrokerId, out _brokerId);
                    }
                }

                BrokerName = (await BrokerService.GetBroker(_brokerId)).Name;
                CustomerCount = await CustomerService.GetCustomerCount(_brokerId);

                var invitesSentAndConverted = await ChartDataService.GetInvitesSentAndConvertedSequence(_brokerId);

                var dates = new List<string>();
                dates = invitesSentAndConverted.Select(x => x.Item1.ToShortDateString()).ToList();

                foreach (var date in dates)
                {
                    _lineConfig.Data.Labels.Add(date);
                }


                var listSent = new List<int>();
                var listConverted = new List<int>();
                listSent = invitesSentAndConverted.Select(x => x.Item2).ToList();
                listConverted = invitesSentAndConverted.Select(x => x.Item3).ToList();

                _SentDataSet.AddRange(listSent);
                _ConvertedDataSet.AddRange(listConverted);

                _lineConfig.Data.Datasets.Add(_ConvertedDataSet);
                _lineConfig.Data.Datasets.Add(_SentDataSet);

                var totalLogins = await ChartDataService.GetTotalLogins(_brokerId);

                var loginCount = totalLogins.Select(x => x.Item1).ToList();
                var totalLoginsByAmount = totalLogins.Select(x => x.Item2).ToList();

                foreach (var login in loginCount)
                {
                    _steppedConfig.Data.Labels.Add(login.ToString());
                }

                _SentSteppedDataSet.AddRange(totalLoginsByAmount);
                _steppedConfig.Data.Datasets.Add(_SentSteppedDataSet);


                var datesNotifs = new List<string>();
                datesNotifs = invitesSentAndConverted.Select(x => x.Item1.ToShortDateString()).ToList();
                var emailInvitesSent = new List<int>();
                var telephoneInvitesSent = new List<int>();
                var appConversionsLoginEmail = new List<int>();
                var appConversionsLoginTelephone = new List<int>();
                var unconvertedList = new List<int>();
                emailInvitesSent = invitesSentAndConverted.Select(x => x.Item2).ToList();
                telephoneInvitesSent = invitesSentAndConverted.Select(x => x.Item3).ToList();
                appConversionsLoginEmail = invitesSentAndConverted.Select(x => x.Item4).ToList();
                appConversionsLoginTelephone = invitesSentAndConverted.Select(x => x.Item5).ToList();
                unconvertedList = invitesSentAndConverted.Select(x => x.Item6).ToList();

                var EmailDataSet = new BarDataset<int>(emailInvitesSent)
                {
                    Label = "Email Invites Sent Today",
                    BackgroundColor = ColorUtil.FromDrawingColor(SampleUtils.ChartColors.BIQYellow)
                };

                var TelephoneDataSet = new BarDataset<int>(telephoneInvitesSent)
                {
                    Label = "Telephone Invites Sent Today",
                    BackgroundColor = ColorUtil.FromDrawingColor(SampleUtils.ChartColors.BIQLightGray)
                };

                var SignupAfterInviteDataSetEmail = new BarDataset<int>(appConversionsLoginEmail)
                {
                    Label = "Sign up after invite email",
                    BackgroundColor = ColorUtil.FromDrawingColor(SampleUtils.ChartColors.BIQLightGray)
                };

                var SignupAfterInviteDataSetTelephone = new BarDataset<int>(appConversionsLoginTelephone)
                {
                    Label = "Sign up after invite telephone",
                    BackgroundColor = ColorUtil.FromDrawingColor(SampleUtils.ChartColors.BIQLightGray)
                };

                var unconverted = new BarDataset<int>(unconvertedList)
                {
                    Label = "Signup not invited or used a different email/ telephone",
                    BackgroundColor = ColorUtil.FromDrawingColor(SampleUtils.ChartColors.Black)
                };

                foreach (var date in datesNotifs)
                {
                    _barConfig.Data.Labels.Add(date);
                }

                _barConfig.Data.Datasets.Add(EmailDataSet);
                _barConfig.Data.Datasets.Add(TelephoneDataSet);
                _barConfig.Data.Datasets.Add(SignupAfterInviteDataSetEmail);
                _barConfig.Data.Datasets.Add(SignupAfterInviteDataSetTelephone);
                _barConfig.Data.Datasets.Add(unconverted);


                var totals = await this.ChartDataService.GetInvitesSentAndConverted(_brokerId);

                _PieDataSet = new PieDataset<int>(new List<int> { totals.TotalConvertedLoginsEmail, totals.TotalConvertedLoginsTelephone, totals.TotalUnConvertedLogins })
                {
                    BackgroundColor = SampleUtils.ChartColors.All.Take(3).Select(ColorUtil.FromDrawingColor).ToArray(),
                };

                _pieConfig.Data.Datasets.Add(_PieDataSet);
                _pieConfig.Data.Labels.Add("Converted from email");
                _pieConfig.Data.Labels.Add("Converted from telephone");
                _pieConfig.Data.Labels.Add("Chose broker");

                _PieDataSet2 = new PieDataset<int>(new List<int> { totals.TotalConvertedLoginsEmail, totals.TotalConvertedLoginsTelephone, totals.TotalEmailInvites + totals.TotalTelephoneInvites - totals.TotalConvertedLoginsEmail - totals.TotalConvertedLoginsTelephone })
                {
                    BackgroundColor = SampleUtils.ChartColors.All.Take(2).Select(ColorUtil.FromDrawingColor).ToArray(),

                };

                _pieConfig2.Data.Datasets.Add(_PieDataSet2);
                _pieConfig2.Data.Labels.Add("Loggged in after email invite");
                _pieConfig2.Data.Labels.Add("Loggged in after telephone invite");
                _pieConfig2.Data.Labels.Add("Not logged in yet");

            }
            catch
            {
                StatusClass = "alert-danger";
                Message = "Something went wrong getting details";
                Saved = false;
            }
        }

        protected void NavigateToOverview()
        {
            NavigationManager.NavigateTo($"/brokerlist");
        }
    }
}
