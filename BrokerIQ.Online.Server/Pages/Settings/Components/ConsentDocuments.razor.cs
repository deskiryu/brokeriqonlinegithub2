using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BrokerIQ.Online.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using MudBlazor;
using BrokerIQ.Online.Server.AppSettings;
using BrokerIQ.Online.Services.Interface;
using BrokerIQ.Dto.Models;
using Microsoft.JSInterop;
using BrokerIQ.Online.Server.Extensions;
using BrokerIQ.Online.Server.Components;

namespace BrokerIQ.Online.Server.Pages.Settings.Components
{
    public partial class ConsentDocuments
    {
        [Parameter]
        public User User { get; set; }

        [Parameter]
        public Online.Models.Broker Broker { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        public IBrokerConsentDocumentService BrokerConsentDocumentService { get; set; }

        [Inject]
        public IOptions<FileUploadSettings> FileUploadSettingsOption { get; set; }

        [Inject]
        private IJSRuntime js { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        protected FileUploadSettings FileUploadSettings { get; set; }

        protected MudForm form;

        protected class ConsentDocumentModel
        {
            public Dto.Enum.ConsentDocumentsEnum ConsentType { get; set; }
            public string Description { get; set; }
            public bool IsUrlConsent { get; set; }
            public string Url { get; set; }
            public int MajorVersion { get; set; } = 1;
            public int MinorVersion { get; set; } = 1;
        }

        protected ConsentDocumentModel Model { get; set; } = new ConsentDocumentModel();

        protected List<IBrowserFile> SelectedFiles = new();
        protected string HoverClass;
        protected string DropZoneClass => SelectedFiles.Any() ? "card" : "card";

        protected bool HasExistingFileForEdit { get; set; }
        protected string ExistingFileName { get; set; }

        private IEnumerable<BrokerConsentDocumentDto> ExistingConsents { get; set; } = Enumerable.Empty<BrokerConsentDocumentDto>();

        private bool IsEditing { get; set; }
        private BrokerConsentDocumentDto EditingDocument { get; set; }
        private bool ShowForm { get; set; }

        private int? GetDocumentId(BrokerConsentDocumentDto doc)
        {
            try
            {
                // Prefer strong property if available
                var idProp = doc?.GetType().GetProperty("Id");
                if (idProp != null)
                {
                    var val = idProp.GetValue(doc);
                    if (val is int i) return i;
                    if (val != null && int.TryParse(val.ToString(), out var parsed)) return parsed;
                }
            }
            catch { }
            return null;
        }

        private Dto.Enum.ConsentDocumentsEnum? GetDocumentConsentType(BrokerConsentDocumentDto doc)
        {
            var val = GetProp(doc, "ConsentType", "ConsentDocumentType", "ConsentDocumentsEnum", "ConsentDocumentEnum", "ConsentTypeId", "ConsentDocumentTypeId", "Type", "TypeId");
            try
            {
                if (val == null) return null;
                if (val.GetType().IsEnum)
                {
                    return (Dto.Enum.ConsentDocumentsEnum)val;
                }
                if (val is int iv) return (Dto.Enum.ConsentDocumentsEnum)iv;
                if (int.TryParse(val.ToString(), out var parsed)) return (Dto.Enum.ConsentDocumentsEnum)parsed;
                if (val is string s && Enum.TryParse<Dto.Enum.ConsentDocumentsEnum>(s, out var parsedEnum)) return parsedEnum;
            }
            catch { }
            return null;
        }

        private bool ConsentTypeExists(Dto.Enum.ConsentDocumentsEnum type, int? excludeId = null)
        {
            foreach (var doc in ExistingConsents ?? Enumerable.Empty<BrokerConsentDocumentDto>())
            {
                var docId = GetDocumentId(doc);
                if (excludeId.HasValue && docId.HasValue && docId.Value == excludeId.Value) continue;
                var docType = GetDocumentConsentType(doc);
                if (docType.HasValue && docType.Value.Equals(type))
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnInitialized()
        {
            FileUploadSettings = FileUploadSettingsOption.Value;
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadConsents();
        }

        protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";
        protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

        protected void LoadFiles(InputFileChangeEventArgs e)
        {
            SelectedFiles.Clear();

            try
            {
                var file = e.File;
                var ext = System.IO.Path.GetExtension(file.Name);
                if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Only PDF files are allowed");
                }

                SelectedFiles.Add(file);
            }
            catch (Exception)
            {
                // swallow and keep empty selection
            }
        }

        protected void DeleteSelectedFile()
        {
            SelectedFiles.Clear();
        }

        protected void RemoveExistingFile()
        {
            HasExistingFileForEdit = false;
            StateHasChanged();
        }

        private string GetConsentTypeDescription(Dto.Enum.ConsentDocumentsEnum type)
        {
            // Prefer the enum DisplayAttribute.Prompt as a description; fallback to display name or enum name
            var prompt = type.GetDisplayPrompt();
            if (!string.IsNullOrWhiteSpace(prompt)) return prompt;
            var name = type.GetDisplayName();
            return string.IsNullOrWhiteSpace(name) ? type.ToString() : name;
        }

        protected void OnConsentTypeChanged(Dto.Enum.ConsentDocumentsEnum value)
        {
            Model.ConsentType = value;
            // Auto-fill description from enum when type changes; user may edit afterwards
            Model.Description = GetConsentTypeDescription(value);
            StateHasChanged();
        }

        private async Task LoadConsents()
        {
            if (Broker?.Id > 0)
            {
                ExistingConsents = await BrokerConsentDocumentService.GetForBroker(Broker.Id);
            }
            else
            {
                ExistingConsents = await BrokerConsentDocumentService.GetAllForCurrentBroker();
            }

            StateHasChanged();
        }

        private object GetProp(object obj, params string[] names)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n);
                if (p != null)
                {
                    return p.GetValue(obj);
                }
            }
            return null;
        }

        private int GetInt(object value, int fallback = 0)
        {
            if (value == null) return fallback;
            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value is short s) return s;
            if (value is byte b) return b;
            if (int.TryParse(value.ToString(), out var parsed)) return parsed;
            return fallback;
        }

        private bool GetBool(object value, bool fallback = false)
        {
            if (value == null) return fallback;
            if (value is bool b) return b;
            if (bool.TryParse(value.ToString(), out var parsed)) return parsed;
            return fallback;
        }

        private string GetString(object value)
        {
            return value?.ToString() ?? string.Empty;
        }

        // Helpers used by the Razor table
        protected string GetConsentTypeLabel(BrokerConsentDocumentDto doc)
        {
            var val = GetProp(doc, "ConsentType", "ConsentDocumentType", "ConsentDocumentsEnum", "ConsentDocumentEnum", "ConsentTypeId", "ConsentDocumentTypeId", "Type", "TypeId");
            try
            {
                if (val != null && val.GetType().IsEnum)
                {
                    var name = ((Enum)val).GetDisplayName();
                    return string.IsNullOrWhiteSpace(name) ? val.ToString() : name;
                }
                if (val is int iv)
                {
                    var enumVal = (Dto.Enum.ConsentDocumentsEnum)iv;
                    var name = enumVal.GetDisplayName();
                    return string.IsNullOrWhiteSpace(name) ? enumVal.ToString() : name;
                }
                if (val != null && int.TryParse(val.ToString(), out var pv))
                {
                    var enumVal = (Dto.Enum.ConsentDocumentsEnum)pv;
                    var name = enumVal.GetDisplayName();
                    return string.IsNullOrWhiteSpace(name) ? enumVal.ToString() : name;
                }
                // try parsing by name
                if (val is string s && Enum.TryParse<Dto.Enum.ConsentDocumentsEnum>(s, out var parsed))
                {
                    var name = parsed.GetDisplayName();
                    return string.IsNullOrWhiteSpace(name) ? parsed.ToString() : name;
                }
                return val?.ToString() ?? "";
            }
            catch { return val?.ToString() ?? ""; }
        }

        protected bool CanView(BrokerConsentDocumentDto doc)
        {
            var url = GetString(GetProp(doc, "Url"));
            var file = GetProp(doc, "File") as byte[];
            return (!string.IsNullOrWhiteSpace(url) && url.IsValidUrl()) || (file != null && file.Length > 0);
        }

        protected async Task ViewConsent(BrokerConsentDocumentDto doc)
        {
            var url = GetString(GetProp(doc, "Url"));
            if (!string.IsNullOrWhiteSpace(url) && url.IsValidUrl())
            {
                await ExtensionClass.OpenLinkInNewTab(js, url);
                return;
            }

            var file = GetProp(doc, "File") as byte[];
            if (file != null && file.Length > 0)
            {
                await ExtensionClass.PreviewFile(js, new MemoryStream(file));
            }
        }

        protected async Task PreviewCurrent()
        {
            if (Model.IsUrlConsent)
            {
                var url = Model.Url?.Trim();
                if (!string.IsNullOrWhiteSpace(url) && url.IsValidUrl())
                {
                    await ExtensionClass.OpenLinkInNewTab(js, url);
                }
                else
                {
                    // try existing document url if editing
                    var existingUrl = EditingDocument != null ? GetString(GetProp(EditingDocument, "Url")) : null;
                    if (!string.IsNullOrWhiteSpace(existingUrl) && existingUrl.IsValidUrl())
                    {
                        await ExtensionClass.OpenLinkInNewTab(js, existingUrl);
                    }
                    else
                    {
                        Snackbar.Add("Please enter a valid URL.", Severity.Warning);
                    }
                }
            }
            else if (SelectedFiles.Any())
            {
                try
                {
                    var file = SelectedFiles.First();
                    using var ms = new MemoryStream();
                    await file.OpenReadStream(FileUploadSettings.MaxFileSize).CopyToAsync(ms);
                    await ExtensionClass.PreviewFile(js, ms);
                }
                catch
                {
                    Snackbar.Add("Unable to preview the selected file.", Severity.Error);
                }
            }
            else if (EditingDocument != null)
            {
                var url = GetString(GetProp(EditingDocument, "Url"));
                if (!string.IsNullOrWhiteSpace(url) && url.IsValidUrl())
                {
                    await ExtensionClass.OpenLinkInNewTab(js, url);
                    return;
                }
                var file = GetProp(EditingDocument, "File") as byte[];
                if (file != null && file.Length > 0)
                {
                    await ExtensionClass.PreviewFile(js, new MemoryStream(file));
                }
            }
        }

        protected bool CanPreviewCurrent()
        {
            if (Model.IsUrlConsent)
            {
                var url = Model.Url?.Trim();
                if (!string.IsNullOrWhiteSpace(url) && url.IsValidUrl()) return true;
                // fallback to existing url when editing
                if (EditingDocument != null)
                {
                    var existingUrl = GetString(GetProp(EditingDocument, "Url"));
                    if (!string.IsNullOrWhiteSpace(existingUrl) && existingUrl.IsValidUrl()) return true;
                }
                return false;
            }
            if (SelectedFiles.Any()) return true;
            if (EditingDocument != null)
            {
                var file = GetProp(EditingDocument, "File") as byte[];
                if (file != null && file.Length > 0) return true;
            }
            return false;
        }

        protected string GetDescription(BrokerConsentDocumentDto doc)
        {
            return GetString(GetProp(doc, "Description"));
        }

        protected string GetVersionLabel(BrokerConsentDocumentDto doc)
        {
            var maj = GetInt(GetProp(doc, "MajorVersion"), 0);
            var min = GetInt(GetProp(doc, "MinorVersion"), 0);
            return $"v{maj}.{min:D2}";
        }

        protected string GetBracketedVersion(BrokerConsentDocumentDto doc)
        {
            var maj = GetInt(GetProp(doc, "MajorVersion"), 0);
            var min = GetInt(GetProp(doc, "MinorVersion"), 0);
            return $"[{maj}.{min:D2}]";
        }

        protected string GetSourceLabel(BrokerConsentDocumentDto doc)
        {
            var isUrl = GetBool(GetProp(doc, "IsUrlConsent"));
            if (isUrl)
            {
                var url = GetString(GetProp(doc, "Url"));
                return string.IsNullOrWhiteSpace(url) ? "URL" : url;
            }
            var fileName = GetString(GetProp(doc, "FileName"));
            return string.IsNullOrWhiteSpace(fileName) ? "PDF" : fileName;
        }

        protected async Task ConfirmDelete(BrokerConsentDocumentDto doc)
        {
            var parameters = new DialogParameters
            {
                { "ContentText", "Are you sure you wish to delete this consent requirement?" },
                { "ButtonText", "Delete" },
                { "Color", Color.Error }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
            var result = await DialogService.Show<ConfirmationDialog>("Delete", parameters, options).Result;
            if (result.Canceled) return;

            var ok = await BrokerConsentDocumentService.Delete(doc);
            if (ok)
            {
                Snackbar.Add("Consent requirement deleted successfully", Severity.Success);
                if (EditingDocument?.Id == doc.Id)
                {
                    CancelEdit();
                }
                await LoadConsents();
            }
            else
            {
                Snackbar.Add("Unable to delete consent requirement. Please try again.", Severity.Error);
            }
        }

        protected void StartEdit(BrokerConsentDocumentDto doc)
        {
            EditingDocument = doc;
            IsEditing = true;
            ShowForm = true;

            // Populate the form model from the selected document via reflection
            var consentType = GetProp(doc, "ConsentType", "ConsentDocumentType", "ConsentDocumentsEnum", "ConsentDocumentEnum", "ConsentTypeId", "ConsentDocumentTypeId");
            if (consentType != null)
            {
                if (consentType is int iv)
                    Model.ConsentType = (Dto.Enum.ConsentDocumentsEnum)iv;
                else if (int.TryParse(consentType.ToString(), out var pv))
                    Model.ConsentType = (Dto.Enum.ConsentDocumentsEnum)pv;
            }

            Model.Description = GetString(GetProp(doc, "Description"));
            Model.MajorVersion = GetInt(GetProp(doc, "MajorVersion"), 1);
            Model.MinorVersion = GetInt(GetProp(doc, "MinorVersion"), 0);

            var isUrl = GetBool(GetProp(doc, "IsUrlConsent"));
            var url = GetString(GetProp(doc, "Url"));
            Model.IsUrlConsent = isUrl || !string.IsNullOrWhiteSpace(url);
            Model.Url = url;

            // Existing file info for edit mode display/validation
            var fileBytes = GetProp(doc, "File") as byte[];
            var fileName = GetString(GetProp(doc, "FileName"));
            HasExistingFileForEdit = (fileBytes != null && fileBytes.Length > 0) || !string.IsNullOrWhiteSpace(fileName);
            ExistingFileName = string.IsNullOrWhiteSpace(fileName) ? "Existing PDF" : fileName;

            SelectedFiles.Clear();
            StateHasChanged();
        }

        protected void CancelEdit()
        {
            IsEditing = false;
            EditingDocument = null;
            Model = new ConsentDocumentModel();
            SelectedFiles.Clear();
            ShowForm = false;
            StateHasChanged();
        }

        protected void StartAdd()
        {
            IsEditing = false;
            EditingDocument = null;
            Model = new ConsentDocumentModel();
            SelectedFiles.Clear();
            ShowForm = true;
            // Initialize description from the default/initial type so it’s pre-filled on create
            Model.Description = GetConsentTypeDescription(Model.ConsentType);
            StateHasChanged();
        }

        protected async Task Submit()
        {
            await form.Validate();

            // Basic validation: consent type, version numbers, and either URL or file
            if (Model.IsUrlConsent)
            {
                if (string.IsNullOrWhiteSpace(Model.Url))
                {
                    Snackbar.Add("Please provide a valid URL", Severity.Warning);
                    return;
                }
                if (!Uri.TryCreate(Model.Url, UriKind.Absolute, out var _))
                {
                    Snackbar.Add("URL format is invalid", Severity.Warning);
                    return;
                }
            }
            else
            {
                if (!SelectedFiles.Any())
                {
                    // In edit mode, allow keeping existing file without forcing re-upload
                    if (!(IsEditing && HasExistingFileForEdit))
                    {
                        Snackbar.Add("Please upload a PDF file", Severity.Warning);
                        return;
                    }
                }
            }

            // Uniqueness: prevent two consents with same type
            var excludeId = IsEditing && EditingDocument != null ? GetDocumentId(EditingDocument) : (int?)null;
            if (ConsentTypeExists(Model.ConsentType, excludeId))
            {
                var typeName = GetConsentTypeDescription(Model.ConsentType);
                Snackbar.Add($"A consent for '{typeName}' already exists.", Severity.Warning);
                return;
            }

            if (IsEditing && EditingDocument != null)
            {
                // Update existing document using reflection-safe setters
                void SetEditProp(string name, object value)
                {
                    var p = EditingDocument.GetType().GetProperty(name);
                    if (p != null && value != null)
                    {
                        if (p.PropertyType.IsEnum && value.GetType().IsEnum)
                        {
                            p.SetValue(EditingDocument, Enum.ToObject(p.PropertyType, (int)value));
                        }
                        else if (p.PropertyType.IsEnum && value is int iv)
                        {
                            p.SetValue(EditingDocument, Enum.ToObject(p.PropertyType, iv));
                        }
                        else if (!p.PropertyType.IsEnum && value.GetType().IsEnum)
                        {
                            // target expects a numeric type, but we have an enum value
                            var intVal = (int)value;
                            if (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)) p.SetValue(EditingDocument, intVal);
                            else if (p.PropertyType == typeof(short) || p.PropertyType == typeof(short?)) p.SetValue(EditingDocument, (short)intVal);
                            else if (p.PropertyType == typeof(byte) || p.PropertyType == typeof(byte?)) p.SetValue(EditingDocument, (byte)intVal);
                            else p.SetValue(EditingDocument, intVal);
                        }
                        else
                        {
                            p.SetValue(EditingDocument, value);
                        }
                    }
                }

                SetEditProp("ConsentType", Model.ConsentType);
                SetEditProp("ConsentDocumentType", Model.ConsentType);
                SetEditProp("ConsentDocumentsEnum", Model.ConsentType);
                SetEditProp("ConsentDocumentEnum", Model.ConsentType);
                SetEditProp("ConsentTypeId", (int)Model.ConsentType);
                SetEditProp("ConsentDocumentTypeId", (int)Model.ConsentType);

                var desc = (Model.Description ?? string.Empty).Trim();
                if (desc.Length > 300) desc = desc.Substring(0, 300);
                SetEditProp("Description", desc);

                SetEditProp("MajorVersion", Model.MajorVersion);
                SetEditProp("MinorVersion", Model.MinorVersion);

                SetEditProp("IsUrlConsent", Model.IsUrlConsent);
                if (Model.IsUrlConsent)
                {
                    SetEditProp("Url", Model.Url);
                    // If switching to URL, it is okay to leave file properties as-is or blank
                }
                else
                {
                    var file = SelectedFiles.FirstOrDefault();
                    if (file != null)
                    {
                        using var ms = new MemoryStream();
                        await file.OpenReadStream(FileUploadSettings.MaxFileSize).CopyToAsync(ms);
                        var bytes = ms.ToArray();
                        SetEditProp("File", bytes);
                        SetEditProp("FileName", file.Name);
                    }
                }

                var updated = await BrokerConsentDocumentService.Update(EditingDocument);
                if (updated)
                {
                    Snackbar.Add($"Updated consent: {Model.ConsentType} {GetVersionLabel(EditingDocument)}", Severity.Success);
                    CancelEdit();
                    await LoadConsents();
                }
                else
                {
                    Snackbar.Add("Unable to update consent document. Please try again.", Severity.Error);
                }
            }
            else
            {
                // Build the create DTO via reflection to avoid tight coupling to Dto property names
                var dtoType = Type.GetType("BrokerIQ.Dto.Models.CreateBrokerConsentDocumentDto, BrokerIQ.Dto", throwOnError: false);
                if (dtoType == null)
                {
                    Snackbar.Add("Consent document DTO not found.", Severity.Error);
                    return;
                }

                var createDto = Activator.CreateInstance(dtoType);

                void SetProp(string name, object value)
                {
                    var p = dtoType.GetProperty(name);
                    if (p != null && value != null)
                    {
                        // convert enums to underlying type if needed
                        if (p.PropertyType.IsEnum && value.GetType().IsEnum)
                        {
                            p.SetValue(createDto, Enum.ToObject(p.PropertyType, (int)value));
                        }
                        else if (p.PropertyType.IsEnum && value is int iv)
                        {
                            p.SetValue(createDto, Enum.ToObject(p.PropertyType, iv));
                        }
                        else if (!p.PropertyType.IsEnum && value.GetType().IsEnum)
                        {
                            var intVal = (int)value;
                            if (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)) p.SetValue(createDto, intVal);
                            else if (p.PropertyType == typeof(short) || p.PropertyType == typeof(short?)) p.SetValue(createDto, (short)intVal);
                            else if (p.PropertyType == typeof(byte) || p.PropertyType == typeof(byte?)) p.SetValue(createDto, (byte)intVal);
                            else p.SetValue(createDto, intVal);
                        }
                        else
                        {
                            p.SetValue(createDto, value);
                        }
                    }
                }

                // Map known fields with multiple fallbacks for property names
                // Consent type
                SetProp("ConsentType", Model.ConsentType);
                SetProp("ConsentDocumentType", Model.ConsentType);
                SetProp("ConsentDocumentsEnum", Model.ConsentType);
                SetProp("ConsentDocumentEnum", Model.ConsentType);
                SetProp("ConsentTypeId", (int)Model.ConsentType);
                SetProp("ConsentDocumentTypeId", (int)Model.ConsentType);

                // Description
                var desc = (Model.Description ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(desc))
                {
                    desc = GetConsentTypeDescription(Model.ConsentType);
                }
                if (desc.Length > 300) desc = desc.Substring(0, 300);
                SetProp("Description", desc);

                // Versioning
                SetProp("MajorVersion", Model.MajorVersion);
                SetProp("MinorVersion", Model.MinorVersion);

                // URL vs File
                SetProp("IsUrlConsent", Model.IsUrlConsent);
                if (Model.IsUrlConsent)
                {
                    SetProp("Url", Model.Url);
                }
                else
                {
                    var file = SelectedFiles.FirstOrDefault();
                    if (file != null)
                    {
                        using var ms = new MemoryStream();
                        await file.OpenReadStream(FileUploadSettings.MaxFileSize).CopyToAsync(ms);
                        var bytes = ms.ToArray();
                        SetProp("File", bytes);
                        SetProp("FileName", file.Name);
                    }
                }

                // Call service
                var success = await BrokerConsentDocumentService.Create((dynamic)createDto);

                if (success)
                {
                    var summary = $"Created consent: {Model.ConsentType} v{Model.MajorVersion}.{Model.MinorVersion}" +
                                  (Model.IsUrlConsent ? $" (URL: {Model.Url})" : $" (PDF: {SelectedFiles.FirstOrDefault()?.Name})");
                    Snackbar.Add(summary, Severity.Success);
                    // reset state
                    Model = new ConsentDocumentModel();
                    SelectedFiles.Clear();
                    await LoadConsents();
                    ShowForm = false;
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add("Unable to save consent document. Please try again.", Severity.Error);
                }
            }
        }
    }
}
