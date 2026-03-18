using Archetype.Tooling.Server;
using Archetype.Tooling.Server.Handlers;

// ---------------------------------------------------------------------------
//  Archetype.Tooling.Server — entry point (D26)
// ---------------------------------------------------------------------------
// Reads newline-delimited JSON-RPC requests from stdin, dispatches to
// registered handlers, and writes responses to stdout.  The Electron main
// process spawns this binary on startup.

var sidecar = new SidecarState();
var server  = new RpcServer(Console.In, Console.Out);

// -----------------------------------------------------------------------
//  Register all 18 RPC methods (D28 method table)
// -----------------------------------------------------------------------

var loadProject          = new LoadProjectHandler(sidecar);
var saveProject          = new SaveProjectHandler(sidecar);
var updateKeywordBody    = new UpdateKeywordBodyHandler(sidecar);
var updateCardEffect     = new UpdateCardEffectHandler(sidecar);
var updateLifetimeSpec   = new UpdateLifetimeSpecHandler(sidecar);
var updateActivationCond = new UpdateActivationConditionHandler(sidecar);
var updateCostBody       = new UpdateCostBodyHandler(sidecar);
var updateField          = new UpdateFieldHandler(sidecar);
var addEntry             = new AddEntryHandler(sidecar);
var removeEntry          = new RemoveEntryHandler(sidecar);
var renameEntry          = new RenameEntryHandler(sidecar);
var getAllDiagnostics     = new GetAllDiagnosticsHandler(sidecar);
var getSymbolInfo        = new GetSymbolInfoHandler(sidecar);
var getReferenceGraph    = new GetReferenceGraphHandler(sidecar);
var getCompletions       = new GetCompletionsHandler(sidecar);
var renderCardText       = new RenderCardTextHandler(sidecar);
var exportGameDef        = new ExportGameDefinitionHandler(sidecar);
var exportGodotClasses   = new ExportGodotClassesHandler(sidecar);

server.Register("LoadProject",              p => loadProject.Handle(p));
server.Register("SaveProject",              p => saveProject.Handle(p));
server.Register("UpdateKeywordBody",        p => updateKeywordBody.Handle(p));
server.Register("UpdateCardEffect",         p => updateCardEffect.Handle(p));
server.Register("UpdateLifetimeSpec",       p => updateLifetimeSpec.Handle(p));
server.Register("UpdateActivationCondition", p => updateActivationCond.Handle(p));
server.Register("UpdateCostBody",           p => updateCostBody.Handle(p));
server.Register("UpdateField",              p => updateField.Handle(p));
server.Register("AddEntry",                 p => addEntry.Handle(p));
server.Register("RemoveEntry",              p => removeEntry.Handle(p));
server.Register("RenameEntry",              p => renameEntry.Handle(p));
server.Register("GetAllDiagnostics",        p => getAllDiagnostics.Handle(p));
server.Register("GetSymbolInfo",            p => getSymbolInfo.Handle(p));
server.Register("GetReferenceGraph",        p => getReferenceGraph.Handle(p));
server.Register("GetCompletions",           p => getCompletions.Handle(p));
server.Register("RenderCardText",           p => renderCardText.Handle(p));
server.Register("ExportGameDefinition",     p => exportGameDef.Handle(p));
server.Register("ExportGodotClasses",       p => exportGodotClasses.Handle(p));

await server.RunAsync();
