# Welland Pool League - Management & Statistics Platform

This repository contains the source code for the Welland Pool League (WPL), a comprehensive web platform for league management and real-time statistics tracking.

**Live Site:** [https://wpl.codersden.com](https://wpl.codersden.com)

---

## About This Project

This web application was developed to modernize and replace a manual, spreadsheet-based system for a local pool league. It provides a centralized, interactive, and full-featured platform for players, captains, and administrators to manage all aspects of the league, from initial schedule creation to final season standings.

---

## Tech Stack

The application is built on a modern .NET stack, leveraging the following technologies:

* **Framework:** **Blazor Server** - For a rich, interactive, and stateful UI with C#.
* **UI Component Library:** **MudBlazor** - A material design component library that provides pre-built, responsive UI elements.
* **Language:** **C#** - The primary language for all front-end and back-end application logic.
* **Database:** **SQL Server** - The relational database used for all league data.
* **Authentication:** **Auth0** - For secure user authentication, management, and role-based access control.

---

## Core Features

The platform is divided into features for league members and powerful tools for administrators.

### League Features (For All Users)

* **Real-Time Standings:** Automatically calculated and updated standings for both individual players and teams.
* **Full Season Schedules:** View the complete weekly and playoff schedules.
* **Team Rosters:** Access up-to-date team and player roster information.
* **User Profile Management:** Users can view and manage their own profile information after logging in.

### Administrative Features

* **Advanced Schedule Management:**
    * **Automatic Schedule Generator:** A powerful tool to generate complete round-robin (single or double) schedules for all active teams based on a start date and game day.
    * **Schedule Validation & Analysis:** The generator validates the created schedule for fairness (e.g., balanced home/away games) and provides detailed statistics before saving.
    * **Manual Game Editor:** Full CRUD (Create, Read, Update, Delete) capabilities for individual games within a schedule.
* **Comprehensive User & Role Management:**
    * **Auth0 Integration:** Securely manage all league members through Auth0.
    * **User Administration:** Create, block/unblock, and delete users directly from the admin dashboard.
    * **Role Assignment:** Dynamically assign roles (e.g., `League_Admin`, `Super_User`) to users to control access to administrative features.
* **Detailed Data Entry:**
    * **Player Game Reporting:** Admins can enter weekly wins and losses for each player in the league.
    * **Content Management:** Full control over league data including Teams, Players, and Bars.

