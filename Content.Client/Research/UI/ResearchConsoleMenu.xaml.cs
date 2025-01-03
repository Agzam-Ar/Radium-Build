using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Research.UI;

[GenerateTypedNameReferences]
public sealed partial class ResearchConsoleMenu : FancyWindow
{
    public Action<string>? OnTechnologyCardPressed;
    public Action? OnServerButtonPressed;

    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    private readonly ResearchSystem _research;
    private readonly SpriteSystem _sprite;
    private readonly AccessReaderSystem _accessReader;

    public EntityUid Entity;

    public ResearchConsoleMenu()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _research = _entity.System<ResearchSystem>();
        _sprite = _entity.System<SpriteSystem>();
        _accessReader = _entity.System<AccessReaderSystem>();

        ServerButton.OnPressed += _ => OnServerButtonPressed?.Invoke();
    }

    public void SetEntity(EntityUid entity)
    {
        Entity = entity;
    }

    // ATARAXIA-EDIT START
    public void UpdatePanels(ResearchConsoleBoundInterfaceState state)
    {
        TechnologyCardsContainer.Children.Clear();

        var availableTech = _research.GetAvailableTechnologies(Entity);
        SyncTechnologyList(AvailableCardsContainer, availableTech);

        if (!_entity.TryGetComponent(Entity, out TechnologyDatabaseComponent? database))
            return;

        var hasAccess = _player.LocalEntity is not { } local ||
                        !_entity.TryGetComponent<AccessReaderComponent>(Entity, out var access) ||
                        _accessReader.IsAllowed(local, Entity, access);

        var techList = database.CurrentTechnologyCards.ToList();

        for (int i = 0; i < techList.Count; i++)
        {
            var tech = _prototype.Index<TechnologyPrototype>(techList[i]);
            var cardControl = new TechnologyCardControl(tech, _prototype, _sprite,
                _research.GetTechnologyDescription(tech, includeTier: false),
                state.Points, hasAccess);

            var currentIndex = techList[i];

            cardControl.OnPressed += () => OnTechnologyCardPressed?.Invoke(currentIndex);

            if (i % 2 == 0)
            {
                var rowContainer = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };
                TechnologyCardsContainer.AddChild(rowContainer);
                rowContainer.AddChild(cardControl);
            }
            else
            {
                (TechnologyCardsContainer.Children.Last() as BoxContainer)?.AddChild(cardControl);
            }
        }

        var unlockedTech = database.UnlockedTechnologies.Select(x => _prototype.Index<TechnologyPrototype>(x));
        SyncTechnologyList(UnlockedCardsContainer, unlockedTech);
    }

    /// <summary>
    ///     Synchronize a container for technology cards with a list of technologies,
    ///     creating or removing UI cards as appropriate.
    /// </summary>
    /// <param name="container">The container which contains the UI cards</param>
    /// <param name="technologies">The current set of technologies for which there should be cards</param>
    public void UpdateInformationPanel(ResearchConsoleBoundInterfaceState state)
    {
        var amountMsg = new FormattedMessage();
        amountMsg.AddMarkup(Loc.GetString("research-console-menu-research-points-text", ("points", state.Points)));
        ResearchAmountLabel.SetMessage(amountMsg);

        if (!_entity.TryGetComponent(Entity, out TechnologyDatabaseComponent? database))
            return;
    }

    private void SyncTechnologyList(BoxContainer container, IEnumerable<TechnologyPrototype> technologies)
    {
        // For the cards which already exist, build a map from technology prototype to the UI card
        var currentTechControls = new Dictionary<TechnologyPrototype, Control>();
        foreach (var child in container.Children)
        {
            if (child is MiniTechnologyCardControl miniCard)
            {
                currentTechControls.Add(miniCard.Technology, child);
            }
        }

        foreach (var tech in technologies)
        {
            if (!currentTechControls.ContainsKey(tech))
            {
                // Create a card for any technology which doesn't already have one.
                var mini = new MiniTechnologyCardControl(tech, _prototype, _sprite, _research.GetTechnologyDescription(tech));
                container.AddChild(mini);
            }
            else
            {
                // The tech already exists in the UI; remove it from the set, so we won't revisit it below
                currentTechControls.Remove(tech);
            }
        }

        // Now, any items left in the dictionary are technologies which were previously
        // available, but now are not. Remove them.
        foreach (var (tech, techControl) in currentTechControls)
        {
            container.Children.Remove(techControl);
        }
    }    // ATARAXIA-EDIT END
}
