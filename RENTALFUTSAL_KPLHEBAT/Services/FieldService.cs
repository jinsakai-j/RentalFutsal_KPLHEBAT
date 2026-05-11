using FutsalRentalConsoleApp.Models;
using RENTALFUTSAL_KPLHEBAT;

namespace FutsalRentalConsoleApp.Services;

public sealed class FieldService
{
    private readonly JsonDataStore<Field> _store;

    public FieldService(JsonDataStore<Field> store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public List<Field> GetActiveFields()
    {
        return _store.Load().Where(field => field.IsActive).OrderBy(field => field.Id).ToList();
    }

    public Field? GetById(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return _store.Load().FirstOrDefault(field => field.Id == id && field.IsActive);
    }

    public OperationResult<Field> AddField(string name, decimal pricePerHour)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<Field>.Fail("Nama lapangan tidak boleh kosong.");
        }

        if (pricePerHour <= 0)
        {
            return OperationResult<Field>.Fail("Harga per jam harus lebih dari 0.");
        }

        List<Field> fields = _store.Load();
        int nextId = fields.Count == 0 ? 1 : fields.Max(field => field.Id) + 1;

        Field newField = new()
        {
            Id = nextId,
            Name = name.Trim(),
            PricePerHour = pricePerHour,
            IsActive = true
        };

        fields.Add(newField);
        _store.Save(fields);

        return OperationResult<Field>.Ok(newField, "Lapangan berhasil ditambahkan.");
    }
}
