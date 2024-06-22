namespace LauncherGamePlugin.Forms;

public interface IFormValidator
{
    // Throws on failed validation
    public void Validate(FormEntry entry);
}

public class NotEmptyValidation : IFormValidator
{
    public void Validate(FormEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Value))
            throw new Exception($"{Utils.OnlyLetters(entry.Name)} is empty");
    }
}

public class ExistsValidation : IFormValidator
{
    public void Validate(FormEntry entry)
    {
        if (entry is FilePickerElement filePickerElement)
        {
            if (!File.Exists(entry.Value))
                throw new Exception($"{Utils.OnlyLetters(filePickerElement.Name)} file does not exist on disk");
        }
        
        if (entry is FolderPickerElement folderPickerElement)
        {
            if (!Directory.Exists(entry.Value))
                throw new Exception($"{Utils.OnlyLetters(folderPickerElement.Name)} folder does not exist on disk");
        }
    }
}

public class ContainsValidation : IFormValidator
{
    private string _contians;
    
    public ContainsValidation(string contains)
    {
        _contians = contains;
    }
    
    public void Validate(FormEntry entry)
    {
        if (!entry.Value.Contains(_contians))
            throw new Exception($"{Utils.OnlyLetters(entry.Name)} does not contain {_contians}");
    }
}

public class MultiContainsValidation : IFormValidator
{
    private string[] _contians;
    
    public MultiContainsValidation(params string[] contains)
    {
        _contians = contains;
    }
    
    public void Validate(FormEntry entry)
    {
        if (_contians.All(contains => !entry.Value.Contains(contains)))
            throw new Exception($"{Utils.OnlyLetters(entry.Name)} does not contain one of the following: {_contians}");
    }
}

public class WhenNotEmptyValidation : IFormValidator
{
    private IEnumerable<IFormValidator> _validators;

    public WhenNotEmptyValidation(IEnumerable<IFormValidator> validators)
    {
        _validators = validators;
    }

    public void Validate(FormEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Value))
        {
            entry.Value = string.Empty;
            return;
        }

        foreach (var validator in _validators)
        {
            validator.Validate(entry);
        }
    }
}

public static class FormEntryExtensions
{
    public static FormEntry NotEmpty(this FormEntry formEntry)
    {
        formEntry.Validators.Add(new NotEmptyValidation());
        return formEntry;
    }

    public static FormEntry Exists(this FormEntry formEntry)
    {
        formEntry.Validators.Add(new ExistsValidation());
        return formEntry;
    }

    public static FormEntry Contains(this FormEntry formEntry, string contains)
    {
        formEntry.Validators.Add(new ContainsValidation(contains));
        return formEntry;
    }

    public static FormEntry Contains(this FormEntry formEntry, string[] contains)
    {
        formEntry.Validators.Add(new MultiContainsValidation(contains));
        return formEntry;
    }

    public static FormEntry WhenNotEmpty(this FormEntry formEntry, params IFormValidator[] entries)
    {
        formEntry.Validators.Add(new WhenNotEmptyValidation(entries));
        return formEntry;
    }
}