using UnityEngine;
using System.Collections.Generic;
namespace CortoTesting
{
    public class FakeCombatantResolver : ICombatantResolver
    {

        private Dictionary<string, CombatantEncounterData> allCombatantsDict = new();
        private CardCatalogSO cardCatalog;
        private DeckSO fakeDeck;

        public IReadOnlyDictionary<string, CombatantEncounterData> AllCombatantsDict { get => allCombatantsDict; }

        private void InitializeFakeCombatResolver()
        {
            // create the fake card catalog
            List<CardSO> cards = new();
            cards.Add(CreateFakeBasicDamageCard());
            cards.Add(CreateFakeDefenseCard());
            cards.Add(CreateFakeStatusEffectCard());
            cardCatalog = CreateFakeCardCatalog(cards);

            // create a fake deck with two of each card
            List<CardSO> fakeDeckCards = new();
            foreach (CardSO card in cards)
            {
                fakeDeckCards.Add(card);
                fakeDeckCards.Add(card);
            }
            fakeDeck = CreateFakeDeck(fakeDeckCards);

            // create two combatants with the same deck
            CombatantDataSO firstCombatantData = CreateFakeCombatantCatalogData("Combatant A", "The first guy.", fakeDeck);
            RuntimeCombatant firstCombatantRuntime = firstCombatantData.GetRuntimeCombatant();

            CombatantDataSO secondCombatantData = CreateFakeCombatantCatalogData("Combatant B", "The other guy", fakeDeck);
            RuntimeCombatant secondCombatantRuntime = secondCombatantData.GetRuntimeCombatant();

            CombatantEncounterData firstGuy = new CombatantEncounterData(firstCombatantRuntime, firstCombatantData);
            CombatantEncounterData otherGuy = new CombatantEncounterData(secondCombatantRuntime, secondCombatantData);

            allCombatantsDict[firstGuy.runtimeData.RuntimeID] = firstGuy;
            allCombatantsDict[otherGuy.runtimeData.RuntimeID] = otherGuy;
        }

        private CombatantDataSO CreateFakeCombatantCatalogData(string combatantName, string combatantDescription, DeckSO deck)
        {
            CombatantDataSO newCatalogData = ScriptableObject.CreateInstance<CombatantDataSO>();
            newCatalogData.RuntimeInit(combatantName, combatantDescription, deck);
            return newCatalogData;
        }
        private DeckSO CreateFakeDeck(List<CardSO> cards)
        {
            DeckSO newDeck = ScriptableObject.CreateInstance<DeckSO>();
            newDeck.RuntimeInit(cards);
            return newDeck;
        }

        private DamageEffect CreateFakeDamageEffect(int damageAmount)
        {
            DamageEffect newDamageEffect = ScriptableObject.CreateInstance<DamageEffect>();
            newDamageEffect.RuntimeInit(damageAmount);
            return newDamageEffect;
        }
        private DefenseEffect CreateFakeDefenseEffect(int blockAmount)
        {
            DefenseEffect newDefenseEffect = ScriptableObject.CreateInstance<DefenseEffect>();
            newDefenseEffect.RuntimeInit(blockAmount);
            return newDefenseEffect;
        }

        private StatusEffect CreateFakeStatusEffect(EnumStatusEffect effect)
        {
            StatusEffect newStatusEffect = ScriptableObject.CreateInstance<StatusEffect>();
            newStatusEffect.RuntimeInit(effect, 1);
            return newStatusEffect;
        }

        private CardSO CreateFakeBasicDamageCard()
        {
            CardSO newCard = ScriptableObject.CreateInstance<CardSO>();
            newCard.RuntimeInit
                (
                    "Fake Basic Damage",
                    CreateFakeDamageEffect(10),
                    EnumCardTag.ATTACK,
                    EnumTargetingMode.SINGLE_ENEMY
                );
            return newCard;
        }
        private CardSO CreateFakeDefenseCard()
        {
            CardSO newCard = ScriptableObject.CreateInstance<CardSO>();
            newCard.RuntimeInit
                (
                    "Fake Block",
                    CreateFakeDefenseEffect(5),
                    EnumCardTag.DEFENSE,
                    EnumTargetingMode.SINGLE_COMBATANT

                );
            return newCard;
        }
        private CardSO CreateFakeStatusEffectCard()
        {
            CardSO newCard = ScriptableObject.CreateInstance<CardSO>();
            newCard.RuntimeInit
                (
                    "Fake Frail Status",
                    CreateFakeStatusEffect(EnumStatusEffect.FRAIL),
                    EnumCardTag.STATUS,
                    EnumTargetingMode.SINGLE_ENEMY
                );
            return newCard;
        }

        private CardCatalogSO CreateFakeCardCatalog(List<CardSO> cards)
        {
            CardCatalogSO newCardCatalog = ScriptableObject.CreateInstance<CardCatalogSO>();
            newCardCatalog.RuntimeInit(cards);
            return newCardCatalog;
        }
        private CombatantCatalogSO CreateFakeCombatantCatalog(List<CombatantDataSO> combatants)
        {
            CombatantCatalogSO newCombatantCatalog = ScriptableObject.CreateInstance<CombatantCatalogSO>();
            newCombatantCatalog.RuntimeInit(combatants);
            return newCombatantCatalog;
        }

        ///////////////////////
        // Interface Methods //
        ///////////////////////

        public bool TryGetCombatant(string id, out CombatantEncounterData? data)
        {
            if (allCombatantsDict.ContainsKey(id))
            {
                data = allCombatantsDict[id];
                return true;
            }
            data = null;
            return false;
        }
    }
}


