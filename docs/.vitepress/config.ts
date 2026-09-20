import { defineConfig } from "vitepress";

export default defineConfig({
  title: "PromptPack",
  description: "AI-ready repository context from a local .NET CLI",
  base: process.env.GITHUB_ACTIONS
    ? `/${process.env.GITHUB_REPOSITORY?.split("/")[1] ?? "PromptPack"}/`
    : "/",
  cleanUrls: true,
  appearance: true,
  lastUpdated: true,
  themeConfig: {
    nav: [
      { text: "Guide", link: "/guide/quickstart" },
      { text: "Workflow", link: "/guide/workflow" },
      { text: "CLI reference", link: "/reference/cli" },
    ],
    sidebar: {
      "/guide/": [
        {
          text: "Start here",
          items: [
            { text: "Quickstart", link: "/guide/quickstart" },
            { text: "Architecture", link: "/guide/architecture" },
            {
              text: "Development architecture",
              link: "/guide/development-architecture",
            },
            { text: "Workflow", link: "/guide/workflow" },
          ],
        },
        {
          text: "Reference",
          items: [{ text: "CLI", link: "/reference/cli" }],
        },
      ],
      "/reference/": [
        {
          text: "Reference",
          items: [{ text: "CLI", link: "/reference/cli" }],
        },
      ],
    },
    outline: "deep",
    outlineTitle: "On this page",
    sidebarMenuLabel: "Menu",
    returnToTopLabel: "Return to top",
    darkModeSwitchLabel: "Appearance",
    lightModeSwitchTitle: "Switch to light theme",
    darkModeSwitchTitle: "Switch to dark theme",
    search: { provider: "local" },
    footer: {
      message: "Collect locally. Shape deliberately. Hand off clearly.",
      copyright: "PromptPack",
    },
  },
});
