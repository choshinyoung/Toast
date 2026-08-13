const path = require("path");
const fs = require("fs");
const { workspace } = require("vscode");
const {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
} = require("vscode-languageclient/node");

let client;

function activate(context) {
  console.log("[Toast Extension] Activating Toast Language Client...");

  let serverExecutable;

  // Check workspace for compiled Toast.Tools binary or project
  let exePath = null;
  let projPath = null;

  if (workspace.workspaceFolders && workspace.workspaceFolders.length > 0) {
    for (const folder of workspace.workspaceFolders) {
      const exeCandidate = path.join(
        folder.uri.fsPath,
        "Toast.Tools",
        "bin",
        "Debug",
        "net10.0",
        "Toast.Tools.exe",
      );
      if (fs.existsSync(exeCandidate)) {
        exePath = exeCandidate;
        break;
      }
      const projCandidate = path.join(
        folder.uri.fsPath,
        "Toast.Tools",
        "Toast.Tools.csproj",
      );
      if (fs.existsSync(projCandidate)) {
        projPath = projCandidate;
        break;
      }
    }
  }

  if (exePath && fs.existsSync(exePath)) {
    serverExecutable = {
      command: exePath,
      args: ["--lsp"],
    };
  } else if (projPath && fs.existsSync(projPath)) {
    serverExecutable = {
      command: "dotnet",
      args: ["run", "--project", projPath, "--", "--lsp"],
    };
  } else {
    serverExecutable = {
      command: "toast",
      args: ["--lsp"],
    };
  }

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

  client.start();
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
