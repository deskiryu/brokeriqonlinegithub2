using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

using BrokerIQ.Dto.Enum;
using BrokerIQ.Dto.Models;
using BrokerIQ.Online.Models;
using BrokerIQ.Online.Services.Interface;

using MudBlazor;
using AutoMapper;
using System.Collections.Generic;

namespace BrokerIQ.Online.Server.Pages.Mortgage.Components;

public partial class DocumentAnalyzerMortgageDialog : ComponentBase
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    [Inject]
    public IMortgageService MortgageService { get; set; }

    [Inject]
    public IMapper mapper { get; set; }

    public string HoverClass;

    protected void OnDragEnter(DragEventArgs e) => HoverClass = "drag-file-hover";

    protected void OnDragLeave(DragEventArgs e) => HoverClass = string.Empty;

    public IBrowserFile UploadedFile { get; set; }

    public Online.Models.Mortgage Mortgage { get; set; }

    protected bool IsProcessing { get; set; } = false;

    public bool AnalyzerDisabled => UploadedFile == null || UploadedFile.Size == 0 || IsProcessing;

    protected string FillButtonStyle { get; set; } = "display:none;";

    protected string AnalyzeButtonStyle { get; set; } = string.Empty;

    protected string UploadStyle { get; set; } = string.Empty;

    protected string KeyTableStyle { get; set; } = "display:none;";

    protected Dictionary<string, string> ExtractedValues { get; set; } = new Dictionary<string, string>();

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected async void FillFileContents(InputFileChangeEventArgs e)
    {
        UploadedFile = e.File;

        StateHasChanged();
    }

    private async Task<byte[]> GetFileBytes()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        await using var fileStream = new FileStream(path, FileMode.Create);
        await UploadedFile.OpenReadStream(UploadedFile.Size).CopyToAsync(fileStream);
        var bytes = new byte[UploadedFile.Size];
        fileStream.Position = 0;
        await fileStream.ReadAsync(bytes);
        fileStream.Close();
        File.Delete(path);
        return bytes;
    }

    protected async void AnalyzeDocument()
    {
        IsProcessing = true;

        var fileContent = await GetFileBytes();

        var response = await MortgageService.GetFromFile(new DocumentDto() { File = fileContent });

        ExtractedValues = response.ExtractedValues;

        if (ExtractedValues.Count > 0)
        {
            Mortgage = mapper.Map<Online.Models.Mortgage>(response.Mortgage);

            Mortgage.SupportingDocuments.Clear();

            Mortgage.SupportingDocuments.Add(new MortgageDocument
            {
                FileName = UploadedFile.Name,
                SupportingDocumentType = DocumentTypeEnum.PDF,
                File = fileContent
            });
        }
        else
        {
            UploadedFile = null;
        }

        KeyTableStyle = string.Empty;
        FillButtonStyle = ExtractedValues.Count > 0 ? string.Empty : "display:none;";
        AnalyzeButtonStyle = ExtractedValues.Count > 0 ? "display:none;" : string.Empty;
        UploadStyle = ExtractedValues.Count > 0 ? "display:none;" : string.Empty;

        IsProcessing = false;

        StateHasChanged();
    }

    protected void FillDetails()
    {
        MudDialog.Close(DialogResult.Ok((UploadedFile, Mortgage)));
    }
}