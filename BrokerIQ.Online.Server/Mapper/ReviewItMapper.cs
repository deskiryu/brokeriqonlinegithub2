using System;
using AutoMapper;

namespace BrokerIQ.Online.Mapper
{
    using Dto.Models;
    using Models;
    using BrokerIQ.Dto.Response;
    using BrokerIQ.Online.Models.Account;
    using BrokerIQ.Online.Server.Models;

    public class ReviewItMapper : Profile
    {
        public ReviewItMapper()
        {
            CustomerMapper();
            InsuranceMapper();
            InsuranceDocumentMapper();
            MortgageMapper();
            MortgageDocumentMapper();
            CustomerDocumentMapper();
            LoginMapper();
            BrokerMapper();
            NotificationMapper();
            BrokerStaffMapper();
            EmailInviteMapper();
            MenuPlanMapper();
            NotesMapper();
            ChatMapper();
            AzureStorageMapper();
        }
        public void CustomerMapper()
        {
            CreateMap<CustomerDto, Customer>()
                .ForMember(d => d.Id, action => action.MapFrom(s => s.Id))
                .ForMember(d => d.FirstName, action => action.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, action => action.MapFrom(s => s.LastName))
                .ForMember(d => d.EmailAddress, action => action.MapFrom(s => s.EmailAddress))
                .ForMember(d => d.Salutation, action => action.MapFrom(s => s.Salutation))
                .ForMember(d => d.DateOfBirth, action => action.MapFrom(s => s.DateOfBirth))
                .ForMember(d => d.TelephoneNumber, action => action.MapFrom(s => s.TelephoneNumber))
                .ForMember(d => d.Address, action => action.MapFrom(s => s.Address))
                .ForMember(d => d.Employment, action => action.MapFrom(s => s.Employment))
                .ForMember(d => d.Nationality, action => action.MapFrom(s => s.Nationality))
                .ForMember(d => d.ResidentialStatus, action => action.MapFrom(s => s.ResidentialStatus))
                .ForMember(d => d.OperatingSystem, action => action.MapFrom(s => s.OperatingSystem))
                .ForMember(d => d.LatestVideoUrl, action => action.MapFrom(s => s.LatestMultimediaUrl))
                .ForMember(d => d.ChosenBrokerId, action => action.MapFrom(s => s.ChosenBrokerId))
                .ForMember(d => d.PotentialBroker1, action => action.MapFrom(s => s.PotentialBroker1))
                .ForMember(d => d.PotentialBroker2, action => action.MapFrom(s => s.PotentialBroker2))
                .ForMember(d => d.PotentialBroker3, action => action.MapFrom(s => s.PotentialBroker3))
                .ForMember(d => d.BusinessName, action => action.MapFrom(s => s.BusinessName))
                .ForMember(d => d.EmailConfirmed, action => action.MapFrom(s => s.EmailConfirmed))
                .ForMember(d => d.Insurances, action => action.MapFrom(s => s.Insurances))
                .ForMember(d => d.MenuPlans, action => action.MapFrom(s => s.MenuPlans))
                .ForMember(d => d.ProfilePicture, action => action.MapFrom(s => s.ProfilePicture))
                .ForMember(d => d.Mortgages, action => action.MapFrom(s => s.Mortgages));

            CreateMap<Customer, UpdateCustomerDto>()
                .ForMember(d => d.Id, action => action.MapFrom(s => s.Id))
                .ForMember(d => d.FirstName, action => action.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, action => action.MapFrom(s => s.LastName))
                .ForMember(d => d.Salutation, action => action.MapFrom(s => s.Salutation))
                .ForMember(d => d.DateOfBirth, action => action.MapFrom(s => s.DateOfBirth))
                .ForMember(d => d.TelephoneNumber, action => action.MapFrom(s => s.TelephoneNumber))
                .ForMember(d => d.Address, action => action.MapFrom(s => s.Address))
                .ForMember(d => d.Employment, action => action.MapFrom(s => s.Employment))
                .ForMember(d => d.Nationality, action => action.MapFrom(s => s.Nationality))
                .ForMember(d => d.ResidentialStatus, action => action.MapFrom(s => s.ResidentialStatus))
                .ForMember(d => d.OperatingSystem, action => action.MapFrom(s => s.OperatingSystem))
                .ForMember(d => d.LatestMultimediaUrl, action => action.MapFrom(s => s.LatestVideoUrl))
                .ForMember(d => d.ChosenBrokerId, action => action.MapFrom(s => s.ChosenBrokerId))
                .ForMember(d => d.PotentialBroker1, action => action.MapFrom(s => s.PotentialBroker1))
                .ForMember(d => d.PotentialBroker2, action => action.MapFrom(s => s.PotentialBroker2))
                .ForMember(d => d.PotentialBroker3, action => action.MapFrom(s => s.PotentialBroker3))
                .ForMember(d => d.BusinessName, action => action.MapFrom(s => s.BusinessName));
        }

        public void InsuranceMapper()
        {
            CreateMap<InsuranceDto, Insurance>()
            .ForMember(d => d.TermYears, opt => opt.MapFrom((src, dest) =>
            {
                int? ty = null;
                if (src.TermYears > 0.0m)
                {
                    ty = src.TermYears;
                }
                return ty;
            }))
            .ForMember(d => d.TermAmount, opt => opt.MapFrom((src, dest) =>
            {
                decimal? ta = null;
                if (src.TermAmount > 0.0m)
                {
                    ta = src.TermAmount;
                }
                return ta;
            }))
            .ForMember(d => d.DeferredPeriodWeeks, opt => opt.MapFrom((src, dest) =>
            {
                int? dpw = null;
                if (src.DeferredPeriodWeeks > 0.0m)
                {
                    dpw = src.DeferredPeriodWeeks;
                }
                return dpw;
            }))
            .ForMember(d => d.ReviewDate, opt => opt.MapFrom((src, dest) =>
            {
                DateTime? rd = null;
                if (src.ReviewDate.Year > 2000)
                {
                    rd = src.ReviewDate;
                }
                return rd;
            }));

            CreateMap<Insurance, UpdateInsuranceDto>()
            .ForMember(d => d.TermYears, opt => opt.MapFrom((src, dest) =>
            {
                var ty = 0;
                if (src.TermYears.HasValue)
                {
                    if (src.TermYears.Value > 0.0m)
                    {
                        ty = src.TermYears.Value;
                    }
                }
                return ty;
            }))
            .ForMember(d => d.TermAmount, opt => opt.MapFrom((src, dest) =>
            {
                var ta = 0.0m;
                if (src.TermAmount.HasValue)
                {
                    if (src.TermAmount.Value > 0.0m)
                    {
                        ta = src.TermAmount.Value;
                    }
                }
                return ta;
            }))
            .ForMember(d => d.DeferredPeriodWeeks, opt => opt.MapFrom((src, dest) =>
            {
                var dpw = 0;
                if (src.DeferredPeriodWeeks.HasValue)
                {
                    if (src.DeferredPeriodWeeks.Value > 0.0m)
                    {
                        dpw = src.DeferredPeriodWeeks.Value;
                    }
                }
                return dpw;
            }))
            .ForMember(d => d.ReviewDate, opt => opt.MapFrom((src, dest) =>
            {
                var rd = DateTime.MinValue;
                if (src.ReviewDate.HasValue)
                {
                    if (src.ReviewDate?.Year > 2000)
                    {
                        rd = src.ReviewDate.Value;
                    }
                }
                return rd;
            }));


            CreateMap<Insurance, CreateInsuranceDto>()
            .ForMember(d => d.TermYears, opt => opt.MapFrom((src, dest) =>
            {
                var ty = 0;
                if (src.TermYears.HasValue)
                {
                    if (src.TermYears.Value > 0.0m)
                    {
                        ty = src.TermYears.Value;
                    }
                }
                return ty;
            }))
            .ForMember(d => d.TermAmount, opt => opt.MapFrom((src, dest) =>
            {
                var ta = 0.0m;
                if (src.TermAmount.HasValue)
                {
                    if (src.TermAmount.Value > 0.0m)
                    {
                        ta = src.TermAmount.Value;
                    }
                }
                return ta;
            }))
            .ForMember(d => d.DeferredPeriodWeeks, opt => opt.MapFrom((src, dest) =>
            {
                var dpw = 0;
                if (src.DeferredPeriodWeeks.HasValue)
                {
                    if (src.DeferredPeriodWeeks.Value > 0.0m)
                    {
                        dpw = src.DeferredPeriodWeeks.Value;
                    }
                }
                return dpw;
            }))
            .ForMember(d => d.ReviewDate, opt => opt.MapFrom((src, dest) =>
            {
                var rd = DateTime.MinValue;
                if (src.ReviewDate.HasValue)
                {
                    if (src.ReviewDate?.Year > 2000)
                    {
                        rd = src.ReviewDate.Value;
                    }
                }
                return rd;
            }));
        }

        public void InsuranceDocumentMapper()
        {
            CreateMap<InsuranceDocumentDto, InsuranceDocument>();
            CreateMap<InsuranceDocument, CreateInsuranceDocumentDto>();
        }

        public void MortgageMapper()
        {
            CreateMap<MortgageDto, Mortgage>()
                .ForMember(d => d.Id, action => action.MapFrom(s => s.Id))
                .ForMember(d => d.BrokerId, action => action.MapFrom(s => s.BrokerId))
                .ForMember(d => d.CustomerId, action => action.MapFrom(s => s.CustomerId))
                .ForMember(d => d.ProviderName, action => action.MapFrom(s => s.ProviderName))
                .ForMember(d => d.MortgageType, action => action.MapFrom(s => s.MortgageType))
                .ForMember(d => d.MortgageRateType, action => action.MapFrom(s => s.MortgageRateType))
                .ForMember(d => d.ShowEndDate, action => action.MapFrom(s => s.ShowEndDate))
                .ForMember(d => d.ShowInsight, action => action.MapFrom(s => s.ShowInsight))
                .ForMember(d => d.ShowMonthlyPayment, action => action.MapFrom(s => s.ShowMonthlyPayment))
                .ForMember(d => d.ShowPotentialMonthlyPayment, action => action.MapFrom(s => s.ShowPotentialMonthlyPayment))
                .ForMember(d => d.ShowInterestRate, action => action.MapFrom(s => s.ShowInterestRate))
                .ForMember(d => d.ShowPotentialInterestRate, action => action.MapFrom(s => s.ShowPotentialInterestRate))
                .ForMember(d => d.ShowPotentialEndDate, action => action.MapFrom(s => s.ShowPotentialEndDate))
                .ForMember(d => d.ShowPromotionalEndDate, action => action.MapFrom(s => s.ShowPromotionalEndDate))
                .ForMember(d => d.SupportingDocuments, action => action.MapFrom(s => s.SupportingDocuments))
                .ForMember(d => d.PromotionalEndDate, opt => opt.MapFrom((src, dest) =>
                {
                    DateTime? ped = null;
                    if (src.PromotionalEndDate.Year > 2000)
                    {
                        ped = src.PromotionalEndDate;
                    }
                    return ped;
                }))
                .ForMember(d => d.PotentialEndDate, opt => opt.MapFrom((src, dest) =>
                {
                    DateTime? ped = null;
                    if (src.PotentialEndDate.Year > 2000)
                    {
                        ped = src.PotentialEndDate;
                    }
                    return ped;
                }))
                .ForMember(d => d.EndDate, opt => opt.MapFrom((src, dest) =>
                {
                    DateTime? ed = null;
                    if (src.EndDate.Year > 2000)
                    {
                        ed = src.EndDate;
                    }
                    return ed;
                }))
                .ForMember(d => d.PotentialInterestRate, opt => opt.MapFrom((src, dest) =>
                {
                    decimal? pir = null;
                    if (src.PotentialInterestRate > 0.0m)
                    {
                        pir = src.PotentialInterestRate;
                    }
                    return pir;
                }))
                .ForMember(d => d.PotentialMonthlyPayment, opt => opt.MapFrom((src, dest) =>
                {
                    decimal? pmp = null;
                    if (src.PotentialMonthlyPayment > 0.0m)
                    {
                        pmp = src.PotentialMonthlyPayment;
                    }
                    return pmp;
                }))
                .ForMember(d => d.MonthlyPayment, opt => opt.MapFrom((src, dest) =>
                {
                    decimal? mp = null;
                    if (src.MonthlyPayment > 0.0m)
                    {
                        mp = src.MonthlyPayment;
                    }
                    return mp;
                }))
                .ForMember(d => d.InterestRate, opt => opt.MapFrom((src, dest) =>
                {
                    decimal? pir = null;
                    if (src.InterestRate > 0.0m)
                    {
                        pir = src.InterestRate;
                    }
                    return pir;
                }))
                .ForMember(d => d.MortgageNumber, action => action.MapFrom(s => s.MortgageNumber))
                .ForMember(d => d.ShowMortgageNumber, action => action.MapFrom(s => s.ShowMortgageNumber))
                .ForAllOtherMembers(opt => opt.Ignore());


            CreateMap<Mortgage, UpdateMortgageDto>()
                .ForMember(d => d.Id, action => action.MapFrom(s => s.Id))
                .ForMember(d => d.BrokerId, action => action.MapFrom(s => s.BrokerId))
                .ForMember(d => d.CustomerId, action => action.MapFrom(s => s.CustomerId))
                .ForMember(d => d.ProviderName, action => action.MapFrom(s => s.ProviderName))
                .ForMember(d => d.MortgageType, action => action.MapFrom(s => s.MortgageType))
                .ForMember(d => d.MortgageRateType, action => action.MapFrom(s => s.MortgageRateType))
                .ForMember(d => d.ShowEndDate, action => action.MapFrom(s => s.ShowEndDate))
                .ForMember(d => d.ShowInsight, action => action.MapFrom(s => s.ShowInsight))
                .ForMember(d => d.ShowMonthlyPayment, action => action.MapFrom(s => s.ShowMonthlyPayment))
                .ForMember(d => d.ShowPotentialMonthlyPayment, action => action.MapFrom(s => s.ShowPotentialMonthlyPayment))
                .ForMember(d => d.ShowInterestRate, action => action.MapFrom(s => s.ShowInterestRate))
                .ForMember(d => d.ShowPotentialInterestRate, action => action.MapFrom(s => s.ShowPotentialInterestRate))
                .ForMember(d => d.ShowPotentialEndDate, action => action.MapFrom(s => s.ShowPotentialEndDate))
                .ForMember(d => d.ShowPromotionalEndDate, action => action.MapFrom(s => s.ShowPromotionalEndDate))
                .ForMember(d => d.PromotionalEndDate, opt => opt.MapFrom((src, dest) =>
                {
                    var ped = DateTime.MinValue;
                    if (src.PromotionalEndDate.HasValue)
                    {
                        if (src.PromotionalEndDate?.Year > 2000)
                        {
                            ped = src.PromotionalEndDate.Value;
                        }
                    }
                    return ped;
                }))
                .ForMember(d => d.PotentialEndDate, opt => opt.MapFrom((src, dest) =>
                {
                    var ped = DateTime.MinValue;
                    if (src.PotentialEndDate.HasValue)
                    {
                        if (src.PotentialEndDate?.Year > 2000)
                        {
                            ped = src.PotentialEndDate.Value;
                        }
                    }
                    return ped;
                }))
                .ForMember(d => d.EndDate, opt => opt.MapFrom((src, dest) =>
                {
                    var ed = DateTime.MinValue;
                    if (src.EndDate.HasValue)
                    {
                        if (src.EndDate?.Year > 2000)
                        {
                            ed = src.EndDate.Value;
                        }
                    }
                    return ed;
                }))
                .ForMember(d => d.PotentialInterestRate, opt => opt.MapFrom((src, dest) =>
                {
                    var pir = 0.0m;
                    if (src.PotentialInterestRate.HasValue)
                    {
                        if (src.PotentialInterestRate.Value > 0.0m)
                        {
                            pir = src.PotentialInterestRate.Value;
                        }
                    }
                    return pir;
                }))
                .ForMember(d => d.PotentialMonthlyPayment, opt => opt.MapFrom((src, dest) =>
                {
                    var pmp = 0.0m;
                    if (src.PotentialMonthlyPayment.HasValue)
                    {
                        if (src.PotentialMonthlyPayment.Value > 0.0m)
                        {
                            pmp = src.PotentialMonthlyPayment.Value;
                        }
                    }
                    return pmp;
                }))
                .ForMember(d => d.MonthlyPayment, opt => opt.MapFrom((src, dest) =>
                {
                    var mp = 0.0m;
                    if (src.MonthlyPayment.HasValue)
                    {
                        if (src.MonthlyPayment.Value > 0.0m)
                        {
                            mp = src.MonthlyPayment.Value;
                        }
                    }
                    return mp;
                }))
                .ForMember(d => d.InterestRate, opt => opt.MapFrom((src, dest) =>
                {
                    var ir = 0.0m;
                    if (src.InterestRate.HasValue)
                    {
                        if (src.InterestRate.Value > 0.0m)
                        {
                            ir = src.InterestRate.Value;
                        }
                    }
                    return ir;
                }))
                .ForMember(d => d.MortgageNumber, action => action.MapFrom(s => s.MortgageNumber))
                .ForMember(d => d.ShowMortgageNumber, action => action.MapFrom(s => s.ShowMortgageNumber))
                .ForAllOtherMembers(opt => opt.Ignore());



            CreateMap<Mortgage, CreateMortgageDto>()
                .ForMember(d => d.BrokerId, action => action.MapFrom(s => s.BrokerId))
                .ForMember(d => d.CustomerId, action => action.MapFrom(s => s.CustomerId))
                .ForMember(d => d.ProviderName, action => action.MapFrom(s => s.ProviderName))
                .ForMember(d => d.MortgageType, action => action.MapFrom(s => s.MortgageType))
                .ForMember(d => d.MortgageRateType, action => action.MapFrom(s => s.MortgageRateType))
                .ForMember(d => d.ShowEndDate, action => action.MapFrom(s => s.ShowEndDate))
                .ForMember(d => d.ShowInsight, action => action.MapFrom(s => s.ShowInsight))
                .ForMember(d => d.ShowMonthlyPayment, action => action.MapFrom(s => s.ShowMonthlyPayment))
                .ForMember(d => d.ShowPotentialMonthlyPayment, action => action.MapFrom(s => s.ShowPotentialMonthlyPayment))
                .ForMember(d => d.ShowInterestRate, action => action.MapFrom(s => s.ShowInterestRate))
                .ForMember(d => d.ShowPotentialInterestRate, action => action.MapFrom(s => s.ShowPotentialInterestRate))
                .ForMember(d => d.ShowPotentialEndDate, action => action.MapFrom(s => s.ShowPotentialEndDate))
                .ForMember(d => d.ShowPromotionalEndDate, action => action.MapFrom(s => s.ShowPromotionalEndDate))
                .ForMember(d => d.PromotionalEndDate, opt => opt.MapFrom((src, dest) =>
                {
                    var ped = DateTime.MinValue;
                    if (src.PromotionalEndDate.HasValue)
                    {
                        if (src.PromotionalEndDate?.Year > 2000)
                        {
                            ped = src.PromotionalEndDate.Value;
                        }
                    }
                    return ped;
                }))
                .ForMember(d => d.PotentialEndDate, opt => opt.MapFrom((src, dest) =>
                {
                    var ped = DateTime.MinValue;
                    if (src.PotentialEndDate.HasValue)
                    {
                        if (src.PotentialEndDate?.Year > 2000)
                        {
                            ped = src.PotentialEndDate.Value;
                        }
                    }
                    return ped;
                }))
                .ForMember(d => d.EndDate, opt => opt.MapFrom((src, dest) =>
                {
                    var ed = DateTime.MinValue;
                    if (src.EndDate.HasValue)
                    {
                        if (src.EndDate?.Year > 2000)
                        {
                            ed = src.EndDate.Value;
                        }
                    }
                    return ed;
                }))
                .ForMember(d => d.PotentialInterestRate, opt => opt.MapFrom((src, dest) =>
                {
                    var pir = 0.0m;
                    if (src.PotentialInterestRate.HasValue)
                    {
                        if (src.PotentialInterestRate.Value > 0.0m)
                        {
                            pir = src.PotentialInterestRate.Value;
                        }
                    }
                    return pir;
                }))
                .ForMember(d => d.PotentialMonthlyPayment, opt => opt.MapFrom((src, dest) =>
                {
                    var pmp = 0.0m;
                    if (src.PotentialMonthlyPayment.HasValue)
                    {
                        if (src.PotentialMonthlyPayment.Value > 0.0m)
                        {
                            pmp = src.PotentialMonthlyPayment.Value;
                        }
                    }
                    return pmp;
                }))
                .ForMember(d => d.MonthlyPayment, opt => opt.MapFrom((src, dest) =>
                {
                    var mp = 0.0m;
                    if (src.MonthlyPayment.HasValue)
                    {
                        if (src.MonthlyPayment.Value > 0.0m)
                        {
                            mp = src.MonthlyPayment.Value;
                        }
                    }
                    return mp;
                }))
                .ForMember(d => d.InterestRate, opt => opt.MapFrom((src, dest) =>
                {
                    var ir = 0.0m;
                    if (src.InterestRate.HasValue)
                    {
                        if (src.InterestRate.Value > 0.0m)
                        {
                            ir = src.InterestRate.Value;
                        }
                    }
                    return ir;
                }))
                .ForMember(d => d.MortgageNumber, action => action.MapFrom(s => s.MortgageNumber))
                .ForMember(d => d.ShowMortgageNumber, action => action.MapFrom(s => s.ShowMortgageNumber))
                .ForAllOtherMembers(opt => opt.Ignore());
        }

        public void MortgageDocumentMapper()
        {
            CreateMap<MortgageDocumentDto, MortgageDocument>();
            CreateMap<MortgageDocument, CreateMortgageDocumentDto>();
        }

        public void CustomerDocumentMapper()
        {
            CreateMap<CustomerDocumentDto, CustomerDocument>();
            CreateMap<CustomerDocument, CreateCustomerDocumentDto>();
        }

        public void LoginMapper()
        {
            CreateMap<Login, LoginDto>();
            CreateMap<LoginResponseDto, User>().ForMember(d => d.Token, action => action.MapFrom(s => s.Token))
                            .ForMember(d => d.IsAdmin, action => action.MapFrom(s => s.IsAdmin))
            .ForMember(d => d.IsBroker, action => action.MapFrom(s => s.IsBroker))
            .ForMember(d => d.IsCustomer, action => action.MapFrom(s => s.IsCustomer))
            .ForMember(d => d.IsBrokerStaff, action => action.MapFrom(s => s.IsBrokerStaff))
            .ForMember(d => d.MasterBrokerId, opt => opt.MapFrom((src, dest) =>
            {
                var masterBrokerId = 0;
                if (src.IsBroker)
                {
                    masterBrokerId = src.UserId;
                }
                else if (src.IsBrokerStaff)
                {
                    masterBrokerId = src.MasterBrokerId;
                }

                return masterBrokerId;
            }))
            .ForMember(d => d.StaffBrokerId, opt => opt.MapFrom((src, dest) =>
            {
                int? brokerStaffId = null;
                if (src.IsBrokerStaff)
                {
                    brokerStaffId = src.UserId;
                }
                return brokerStaffId;
            }))
            .ForMember(d => d.Id, action => action.MapFrom(s => s.UserId));
        }

        public void BrokerMapper()
        {
            CreateMap<AddUser, CreateBrokerDto>()
                .ForMember(d => d.Name, action => action.MapFrom(s => s.Name))
                .ForMember(d => d.EmailAddress, action => action.MapFrom(s => s.EmailAddress))
                .ForMember(d => d.BrokerFirstName, action => action.MapFrom(s => s.BrokerFirstName))
                .ForMember(d => d.BrokerLastName, action => action.MapFrom(s => s.BrokerLastName))
                .ForMember(d => d.TelephoneNumber, action => action.MapFrom(s => s.TelephoneNumber))
                .ForMember(d => d.AddressLine1, action => action.MapFrom(s => s.AddressLine1))
                .ForMember(d => d.AddressLine2, action => action.MapFrom(s => s.AddressLine2))
                .ForMember(d => d.AddressLine3, action => action.MapFrom(s => s.AddressLine3))
                .ForMember(d => d.Postcode, action => action.MapFrom(s => s.Postcode))
                .ForMember(d => d.LogoImage, action => action.MapFrom(s => s.LogoImage))
                .ForMember(d => d.Password, action => action.MapFrom(s => s.Password));
            CreateMap<BrokerDto, Broker>();
            CreateMap<Broker, BrokerDto>();
            CreateMap<Broker, UpdateBrokerDto>()
                .ForMember(d => d.Name, action => action.MapFrom(s => s.Name))
                .ForMember(d => d.BrokerFirstName, action => action.MapFrom(s => s.BrokerFirstName))
                .ForMember(d => d.BrokerLastName, action => action.MapFrom(s => s.BrokerLastName))
                .ForMember(d => d.TelephoneNumber, action => action.MapFrom(s => s.TelephoneNumber))
                .ForMember(d => d.AddressLine1, action => action.MapFrom(s => s.AddressLine1))
                .ForMember(d => d.AddressLine2, action => action.MapFrom(s => s.AddressLine2))
                .ForMember(d => d.AddressLine3, action => action.MapFrom(s => s.AddressLine3))
                .ForMember(d => d.Postcode, action => action.MapFrom(s => s.Postcode))
                .ForMember(d => d.LogoImage, action => action.MapFrom(s => s.LogoImage));
        }

        public void BrokerStaffMapper()
        {
            CreateMap<AddStaff, CreateBrokerStaffDto>()
                .ForMember(d => d.EmailAddress, action => action.MapFrom(s => s.EmailAddress))
                .ForMember(d => d.FirstName, action => action.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, action => action.MapFrom(s => s.LastName))
                .ForMember(d => d.Password, action => action.MapFrom(s => s.Password));
            CreateMap<BrokerStaffDto, BrokerStaff>();
            CreateMap<BrokerStaff, BrokerStaffDto>();
            CreateMap<BrokerStaff, UpdateBrokerStaffDto>()
                .ForMember(d => d.FirstName, action => action.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, action => action.MapFrom(s => s.LastName));
        }

        public void NotificationMapper()
        {
            CreateMap<NotificationDto, Notification>();
            CreateMap<BrokerNotificationDto, BrokerNotification>();
        }

        private void EmailInviteMapper()
        {
            CreateMap<EmailInviteDto, EmailInvite>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.EmailAddress, opt => opt.MapFrom(s => s.EmailAddress))
                .ForMember(d => d.BrokerId, opt => opt.MapFrom(s => s.BrokerId))
                .ForMember(d => d.BrokerStaffId, opt => opt.MapFrom(s => s.BrokerStaffId))
                .ForMember(d => d.CreatedDate, opt => opt.MapFrom(s => s.Date))
                .ForMember(d => d.Converted, opt => opt.MapFrom(s => s.Converted))
                .ForMember(d => d.Selected, opt => opt.MapFrom(s => !s.Converted))
                .ForMember(d => d.InvitationCount, opt => opt.MapFrom(s => s.InvitationCount))
                .ForAllOtherMembers(opt => opt.Ignore());
        }

        public void MenuPlanMapper()
        {
            CreateMap<MenuPlanDto, MenuPlan>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.CustomerId, opt => opt.MapFrom(s => s.CustomerId))
            .ForMember(d => d.BrokerId, opt => opt.MapFrom(s => s.BrokerId))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Cost, opt => opt.MapFrom(s => s.Cost))
            .ForMember(d => d.Annual, opt => opt.MapFrom(s => s.Annual))
            .ForMember(d => d.ShowReviewDate, opt => opt.MapFrom(s => s.ShowReviewDate))
            .ForMember(d => d.ReviewDate, opt => opt.MapFrom((src, dest) =>
            {
                DateTime? rd = null;
                if (src.ReviewDate.Year > 2000)
                {
                    rd = src.ReviewDate;
                }
                return rd;
            }));
            CreateMap<MenuPlan, UpdateMenuPlanDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.CustomerId, opt => opt.MapFrom(s => s.CustomerId))
            .ForMember(d => d.BrokerId, opt => opt.MapFrom(s => s.BrokerId))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Cost, opt => opt.MapFrom(s => s.Cost))
            .ForMember(d => d.Annual, opt => opt.MapFrom(s => s.Annual))
            .ForMember(d => d.ShowReviewDate, opt => opt.MapFrom(s => s.ShowReviewDate))
            .ForMember(d => d.ReviewDate, opt => opt.MapFrom((src, dest) =>
            {
                var rd = DateTime.MinValue;
                if (src.ReviewDate.HasValue)
                {
                    if (src.ReviewDate?.Year > 2000)
                    {
                        rd = src.ReviewDate.Value;
                    }
                }
                return rd;
            }));
            CreateMap<MenuPlan, CreateMenuPlanDto>()
            .ForMember(d => d.CustomerId, opt => opt.MapFrom(s => s.CustomerId))
            .ForMember(d => d.BrokerId, opt => opt.MapFrom(s => s.BrokerId))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.Cost, opt => opt.MapFrom(s => s.Cost))
            .ForMember(d => d.Annual, opt => opt.MapFrom(s => s.Annual))
            .ForMember(d => d.ShowReviewDate, opt => opt.MapFrom(s => s.ShowReviewDate))
            .ForMember(d => d.ReviewDate, opt => opt.MapFrom((src, dest) =>
            {
                var rd = DateTime.MinValue;
                if (src.ReviewDate.HasValue)
                {
                    if (src.ReviewDate?.Year > 2000)
                    {
                        rd = src.ReviewDate.Value;
                    }
                }
                return rd;
            }));
        }

        private void NotesMapper()
        {
            CreateMap<Note, NoteDto>();
            CreateMap<CreateNoteDto, Note>();
            CreateMap<NoteDto, Note>();
        }

        private void ChatMapper()
        {
            CreateMap<Chat, ChatDto>()
                .ForMember(p => p.Id, opt => opt.MapFrom(r => r.Id))
                .ForMember(p => p.BrokerId, opt => opt.MapFrom(r => r.BrokerId))
                .ForMember(p => p.CustomerId, opt => opt.MapFrom(r => r.CustomerId))
                .ForMember(p => p.Messages, opt => opt.MapFrom(r => r.Messages));

            CreateMap<ChatDto, Chat>()
                .ForMember(p => p.Id, opt => opt.MapFrom(r => r.Id))
                .ForMember(p => p.BrokerId, opt => opt.MapFrom(r => r.BrokerId))
                .ForMember(p => p.CustomerId, opt => opt.MapFrom(r => r.CustomerId))
                .ForMember(p => p.Messages, opt => opt.MapFrom(r => r.Messages));

            CreateMap<ChatMessageDto, ChatMessage>()
                .ForMember(p => p.Id, opt => opt.MapFrom(r => r.Id))
                .ForMember(p => p.Message, opt => opt.MapFrom(r => r.Message))
                .ForMember(p => p.SentTime, opt => opt.MapFrom(r => r.SentTime))
                .ForMember(p => p.Image, opt => opt.MapFrom(r => r.Image))
                .ForMember(p => p.IsRead, opt => opt.MapFrom(r => r.IsRead))
                .ForMember(p => p.ChatDocumentId, opt => opt.MapFrom(r => r.ChatDocumentId))
                .ForMember(d => d.ChatDocument, opt => opt.MapFrom((src, dest) =>
                {
                    var chatDocument = new ChatDocument();
                    
                    if (src.ChatDocumentId>0)
                    {
                        chatDocument.Id = src.ChatDocument.Id;
                        chatDocument.ChatId = src.ChatDocument.ChatId;
                        chatDocument.ChatMessageId = src.ChatDocument.ChatMessageId;
                        chatDocument.FileName = src.ChatDocument.FileName;
                        chatDocument.File = src.ChatDocument.File;
                        chatDocument.CreatedDate = src.ChatDocument.CreatedDate;
                        chatDocument.SupportingDocumentType = src.ChatDocument.SupportingDocumentType;
                    }
                    return chatDocument;
                }))
                .ForMember(p => p.BrokerSource, opt => opt.MapFrom(r => r.BrokerSource));

            CreateMap<ChatMessage, ChatMessageDto>()
                .ForMember(p => p.Id, opt => opt.MapFrom(r => r.Id))
                .ForMember(p => p.Message, opt => opt.MapFrom(r => r.Message))
                .ForMember(p => p.SentTime, opt => opt.MapFrom(r => r.SentTime))
                .ForMember(p => p.Image, opt => opt.MapFrom(r => r.Image))
                .ForMember(p => p.IsRead, opt => opt.MapFrom(r => r.IsRead))
                .ForMember(p => p.IsRead, opt => opt.MapFrom(r => r.IsRead))
                .ForMember(p => p.BrokerSource, opt => opt.MapFrom(r => r.BrokerSource));
        }

        private void AzureStorageMapper()
        {
            CreateMap<Video, AzureVideoDto>();
            CreateMap<AzureVideoDto, Video>();
            CreateMap<Audio, AudioDto>();
            CreateMap<AudioDto, Audio>();
        }
    }
}
