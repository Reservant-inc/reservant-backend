using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Reservant.Api.Models;

/// <summary>
/// Opening hours for each day of the week
/// </summary>
[SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix")]
public class WeeklyOpeningHours : ICollection<OpeningHours>
{
    private readonly OpeningHours[] _openingHours;
    private int _filled;

    /// <summary>
    /// Construct WeeklyOpeningHours using a list of opening hours for each day
    /// </summary>
    public WeeklyOpeningHours(IEnumerable<OpeningHours> openingHours)
    {
        _openingHours = openingHours.ToArray();
        if (_openingHours.Length != 7)
        {
            throw new ArgumentException(@"Incorrect number of OpeningHours: must be 7", nameof(openingHours));
        }

        _filled = 7;
    }

    /// <summary>
    /// Construct WeeklyOpeningHours with no opening hours for each day
    /// </summary>
    public WeeklyOpeningHours()
    {
        _openingHours = new OpeningHours[7];
        _filled = 0;
    }

    /// <summary>
    /// Get opening hours for the given day of week
    /// </summary>
    public OpeningHours this[DayOfWeek dayOfWeek]
    {
        get => _openingHours[DayOfWeekToIndex(dayOfWeek)];
        set => _openingHours[DayOfWeekToIndex(dayOfWeek)] = value;
    }

    private static int DayOfWeekToIndex(DayOfWeek dayOfWeek) => ((int)dayOfWeek + 1) % 7;

    /// <inheritdoc />
    public IEnumerator<OpeningHours> GetEnumerator() => _openingHours.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _openingHours.GetEnumerator();

    /// <summary>
    /// Remove opening hours for each day of week
    /// </summary>
    public void Clear()
    {
        Array.Fill(_openingHours, new OpeningHours());
    }

    /// <inheritdoc />
    public bool Contains(OpeningHours item) => _openingHours.Contains(item);

    /// <inheritdoc />
    public void CopyTo(OpeningHours[] array, int arrayIndex)
    {
        Array.Copy(_openingHours, array, arrayIndex);
    }

    /// <summary>
    /// To be used by Entity Framework for instantiating the collection.
    ///
    /// Sets the opening hours for week days in sequence, starting from Monday
    /// </summary>
    /// <exception cref="InvalidOperationException">If all week days are already initialized</exception>
    public void Add(OpeningHours item)
    {
        if (_filled >= 7)
        {
            throw new InvalidOperationException("Too many OpeningHours inserted (maximum 7)");
        }

        _openingHours[_filled] = item;
        _filled++;
    }

    /// <summary>
    /// NOT SUPPORTED. Does not make sense for this collection
    /// </summary>
    public bool Remove(OpeningHours item) => throw new NotSupportedException();

    /// <inheritdoc />
    public int Count => 7;

    /// <inheritdoc />
    public bool IsReadOnly => false;
}
