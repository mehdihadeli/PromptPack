---
layout: home
title: PromptPack
titleTemplate: AI-ready repository context
description: Package a local repository into focused, inspectable context for AI tools.
hero:
  name: PromptPack
  text: Repository context, ready for AI
  tagline: Collect the files that matter, shape their contents, and send one predictable context package to your clipboard, a file, or stdout.
  actions:
    - theme: brand
      text: Start packing
      link: /guide/quickstart
    - theme: alt
      text: Read the workflow
      link: /guide/architecture
features:
  - icon: "01"
    title: Select precisely
    details: Combine directory selection, include globs, ignore globs, and .gitignore rules.
  - icon: "02"
    title: Reduce noise
    details: Remove comments and empty lines, or compress implementation detail while keeping structure.
  - icon: "03"
    title: Choose the handoff
    details: Generate XML, Markdown, JSON, or plain text for the tool and workflow you already use.
---

<div class="home-note">
  <strong>Core promise:</strong> useful repository context without making a remote service part of the workflow.
</div>

## Choose your next step

<div class="home-paths">
  <a href="/guide/quickstart" class="home-path">
    <strong>Pack a repository</strong>
    <span>Run one command and copy the result to your clipboard.</span>
  </a>
  <a href="/guide/architecture" class="home-path">
    <strong>Understand the pipeline</strong>
    <span>See how collection, processing, estimation, and generation fit together.</span>
  </a>
  <a href="/reference/cli" class="home-path">
    <strong>Find an option</strong>
    <span>Browse output, filtering, processing, and budget controls in one reference.</span>
  </a>
</div>

## Built for focused context

PromptPack is a local .NET CLI. It reads the repository you point it at, applies selection and processing rules, then creates one merged representation for AI analysis. The default format is Markdown and the default destination is the system clipboard, so the normal loop is short: run `promptpack`, paste, continue.

<div class="home-grid">
  <a href="/guide/quickstart" class="home-grid-item">
    <span class="home-grid-kicker">OUTPUT</span>
    <strong>Four formats</strong>
    <span>Use Markdown by default, or choose XML, JSON, or plain text.</span>
  </a>
  <a href="/guide/workflow" class="home-grid-item">
    <span class="home-grid-kicker">CONTROL</span>
    <strong>Bounded context</strong>
    <span>Show token estimates and fail when a configured budget is exceeded.</span>
  </a>
  <a href="/reference/cli" class="home-grid-item">
    <span class="home-grid-kicker">LOCAL FIRST</span>
    <strong>No upload step</strong>
    <span>PromptPack writes locally, prints locally, or copies locally.</span>
  </a>
</div>
