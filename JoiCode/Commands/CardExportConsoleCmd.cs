using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Joi.JoiCode.Commands;

public class CardExportConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "card-export";
    public override string Args => "<character-id:string> [output-dir:string]";
    public override string Description => "Export all card portraits for a character as rendered PNG images.";
    public override bool IsNetworked => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (args.Length == 0)
        {
            return new CmdResult(false, "No character ID specified. Example: card-export Joi");
        }

        string characterId = args[0];
        string outputDir = args.Length >= 2 ? args[1] : $"card_exports/{characterId}";

        // Find character
        var character = ModelDb.AllCharacters.FirstOrDefault(c =>
            c.Id.Entry.Equals(characterId, StringComparison.OrdinalIgnoreCase));

        if (character == null)
        {
            return new CmdResult(false, $"Character '{characterId}' not found.");
        }

        // Get all cards for this character via CardPool
        var cardPool = character.CardPool;
        if (cardPool == null)
        {
            return new CmdResult(false, $"No card pool found for character '{characterId}'.");
        }

        var cards = cardPool.AllCardIds.Select(id => ModelDb.GetById<CardModel>(id)).ToList();

        if (cards.Count == 0)
        {
            return new CmdResult(false, $"No cards found for character '{characterId}'.");
        }

        Task task = ExportCardsAsync(cards, outputDir, characterId);
        return new CmdResult(task, true, $"Exporting {cards.Count} cards to '{outputDir}'...");
    }

    private async Task ExportCardsAsync(List<CardModel> cards, string outputDir, string characterId)
    {
        var instance = NGame.Instance;
        if (instance == null)
        {
            MainFile.Logger.Error("NGame.Instance is null");
            return;
        }

        // Create output directory
        string fullPath = Path.Combine(OS.GetUserDataDir(), outputDir);
        Directory.CreateDirectory(fullPath);
        MainFile.Logger.Info($"Exporting {cards.Count} cards (base + upgraded) to: {fullPath}");

        // Create a temporary viewport for rendering
        var viewport = new SubViewport();
        viewport.Size = new Vector2I(512, 768); // Card size
        viewport.TransparentBg = true;
        viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;

        instance.AddChild(viewport);

        // Create a single card node and reuse it
        var cardNode = NCard.Create(cards[0], ModelVisibility.Visible);
        if (cardNode == null)
        {
            MainFile.Logger.Error("Failed to create card node (test mode enabled?)");
            viewport.QueueFree();
            return;
        }

        viewport.AddChild(cardNode);

        // Center the card in viewport
        cardNode.Position = new Vector2(
            (viewport.Size.X - cardNode.Size.X) / 2,
            (viewport.Size.Y - cardNode.Size.Y) / 2
        );

        // Prepare metadata
        var metadata = new List<Dictionary<string, object>>();

        try
        {
            int exported = 0;
            foreach (var canonicalCard in cards)
            {
                try
                {
                    // Get localized card name
                    string cardName = canonicalCard.Title ?? canonicalCard.Id.Entry;
                    string cardType = canonicalCard.Type.ToString().ToLowerInvariant();
                    string baseFilename = $"{canonicalCard.Id.Entry.ToLowerInvariant()}.{cardType}.webp";
                    string? upgradedFilename = canonicalCard.IsUpgradable
                        ? $"{canonicalCard.Id.Entry.ToLowerInvariant()}.{cardType}.upgraded.webp"
                        : null;

                    // Add to metadata
                    var cardMeta = new Dictionary<string, object>
                    {
                        ["id"] = canonicalCard.Id.Entry.ToLowerInvariant(),
                        ["name"] = cardName,
                        ["type"] = cardType,
                        ["base"] = baseFilename,
                        ["upgraded"] = upgradedFilename
                    };
                    metadata.Add(cardMeta);

                    // Export base version
                    await ExportSingleCard(instance, viewport, cardNode, canonicalCard, fullPath, false);
                    exported++;

                    // Export upgraded version if the card is upgradable
                    if (canonicalCard.IsUpgradable)
                    {
                        await ExportSingleCard(instance, viewport, cardNode, canonicalCard, fullPath, true);
                        exported++;
                    }

                    // Small delay between cards
                    await instance.ToSignal(instance.GetTree().CreateTimer(0.05), Godot.Timer.SignalName.Timeout);
                }
                catch (Exception ex)
                {
                    MainFile.Logger.Error($"Error exporting card {canonicalCard.Id.Entry}: {ex.Message}");
                }
            }

            // Write metadata to JSON
            string metaPath = Path.Combine(fullPath, "meta.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(metadata, options);
            File.WriteAllText(metaPath, json);
            MainFile.Logger.Info($"Metadata written to: {metaPath}");

            MainFile.Logger.Info($"Export complete: {exported} card images exported to {fullPath}");
        }
        finally
        {
            cardNode.QueueFree();
            viewport.QueueFree();
        }
    }

    private async Task ExportSingleCard(Node instance, SubViewport viewport, NCard cardNode,
        CardModel canonicalCard, string outputPath, bool upgraded)
    {
        // Create a mutable copy for upgrade preview if needed
        CardModel displayCard;
        if (upgraded)
        {
            displayCard = canonicalCard.ToMutable();
            displayCard.UpgradeInternal();
        }
        else
        {
            displayCard = canonicalCard;
        }

        // Update the card node to display this card
        cardNode.Visibility = ModelVisibility.Visible;
        cardNode.Model = displayCard;
        cardNode.UpdateVisuals(PileType.None, upgraded ? CardPreviewMode.Upgrade : CardPreviewMode.Normal);

        // Wait for rendering to complete
        await instance.ToSignal(instance.GetTree(), SceneTree.SignalName.ProcessFrame);
        await instance.ToSignal(instance.GetTree(), SceneTree.SignalName.ProcessFrame);
        await instance.ToSignal(instance.GetTree(), SceneTree.SignalName.ProcessFrame);

        // Capture the viewport
        var image = viewport.GetTexture().GetImage();

        // Get card type
        string cardType = canonicalCard.Type.ToString().ToLowerInvariant();

        // Save as WebP
        string suffix = upgraded ? ".upgraded" : "";
        string filename = $"{canonicalCard.Id.Entry.ToLowerInvariant()}.{cardType}{suffix}.webp";
        string filepath = Path.Combine(outputPath, filename);

        Error err = image.SaveWebp(filepath, false, 0.85f);
        if (err == Error.Ok)
        {
            MainFile.Logger.Info($"Exported: {filename}");
        }
        else
        {
            MainFile.Logger.Error($"Failed to save {filename}: {err}");
        }
    }

    public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
    {
        if (args.Length <= 1)
        {
            var candidates = ModelDb.AllCharacters.Select(c => c.Id.Entry).ToList();
            return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
        }

        return new CompletionResult
        {
            Type = CompletionType.Argument,
            ArgumentContext = CmdName
        };
    }
}
