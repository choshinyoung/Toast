const { workspace } = require("vscode");
const {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
} = require("vscode-languageclient/node");

let client;

function activate(context) {
  console.log("[Toast Extension] Activating Toast Language Client...");

  const serverExecutable = {
    command: "toast",
    args: ["--lsp"],
  };

  const serverOptions = {
    run: serverExecutable,
    debug: serverExecutable,
  };

  const clientOptions = {
    documentSelector: [{ scheme: "file", language: "toast" }],
    synchronize: {
      fileEvents: workspace.createFileSystemWatcher("**/*.toast"),
    },
  };

  client = new LanguageClient("toast", "Toast", serverOptions, clientOptions);

  context.subscriptions.push(client);

  client.start().catch((err) => {
    console.error("[Toast Extension] Failed to start LanguageClient:", err);
  });

  console.log("[Toast Extension] Toast Language Client started.");
}

function deactivate() {
  if (!client) {
    return undefined;
  }
  return client.stop().catch(() => {});
}

module.exports = {
  activate,
  deactivate,
};
