# Welland Pool League Management System

A comprehensive web application for managing pool league operations, statistics, and standings in real-time.

🎯 **Live Site**: [https://wpl.codersden.com](https://wpl.codersden.com)

## 📖 Overview

The Welland Pool League Management System is a modern, interactive web platform designed to replace traditional spreadsheet-based league management. This application provides a centralized solution for tracking player statistics, team standings, schedules, and game results for local pool leagues.

### ✨ Key Benefits
- **Real-time Updates**: Standings and statistics update automatically as scores are entered
- **Mobile Responsive**: Full functionality across desktop and mobile devices  
- **User-friendly Interface**: Clean, intuitive design using Material Design principles
- **Centralized Data**: All league information accessible from a single platform
- **Administrative Control**: Streamlined score entry and league management tools

## 🛠️ Technology Stack

- **Framework**: [Blazor Server](https://docs.microsoft.com/en-us/aspnet/core/blazor/) - Enables rich, interactive UI with C# instead of JavaScript
- **Language**: C# - Full-stack development with a single language
- **UI Components**: [MudBlazor](https://mudblazor.com/) - Material Design component library for Blazor
- **Database**: SQL Server - Robust relational database for league data storage
- **Authentication**: [Auth0](https://auth0.com/) - Secure identity and access management platform
- **Runtime**: .NET Core - Cross-platform, high-performance web framework

## 🚀 Features

### 👥 Team & Player Management
- Create and manage league teams
- Maintain detailed player rosters
- Track player membership and status
- Team performance analytics

### 📊 Real-time Standings
- Automatically calculated league standings
- Live updates as games are completed
- Historical standings tracking
- Team performance metrics

### 🎱 Score Management
- Intuitive score entry interface for administrators
- Weekly game result tracking
- Score validation and error checking
- Game history and statistics

### 📱 Responsive Design
- Optimized for desktop and mobile devices
- Touch-friendly interface for tablets and phones
- Consistent user experience across platforms

### 🔐 Authentication & Authorization
- **Secure User Authentication**: Powered by Auth0 for reliable, secure login
- **Role-based Access Control**: Different permissions for players, captains, and administrators
- **Social Login Support**: Login with Google, Facebook, or traditional email/password
- **Single Sign-On (SSO)**: Seamless authentication experience
- **User Profile Management**: Secure user data and preferences storage

### 🔐 Administrative Features
- Secure admin panel for league management
- Role-based access control via Auth0
- User management and role assignment
- Data export capabilities
- System configuration options

## 🏗️ Project Structure

```
WellandPoolLeagueMud/
├── Components/          # Reusable Blazor components
├── Data/               # Database context and models
├── Pages/              # Blazor pages and routing
├── Services/           # Business logic and data services
├── wwwroot/            # Static files (CSS, JS, images)
├── appsettings.json    # Application configuration
└── Program.cs          # Application entry point
```

## 🚀 Getting Started

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (LocalDB or full installation)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Auth0 Account](https://auth0.com/) - Free tier available for development

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/SithLord2K/WellandPoolLeagueMud.git
   cd WellandPoolLeagueMud
   ```

2. **Configure Auth0 Authentication**
   - Create an Auth0 application in your Auth0 dashboard
   - Set the application type to "Regular Web Application"
   - Configure callback URLs (e.g., `https://localhost:5001/callback`)
   - Add your Auth0 domain and client credentials to `appsettings.json`:
   ```json
   {
     "Auth0": {
       "Domain": "your-domain.auth0.com",
       "ClientId": "your-client-id",
       "ClientSecret": "your-client-secret"
     }
   }
   ```

3. **Configure the database connection**
   - Update the SQL Server connection string in `appsettings.json`
   - Ensure SQL Server is running and accessible

4. **Install dependencies**
   ```bash
   dotnet restore
   ```

5. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

6. **Configure Auth0 Roles (Optional)**
   - In your Auth0 dashboard, create roles: `Player`, `Captain`, `Administrator`
   - Assign users to appropriate roles for testing

7. **Start the application**
   ```bash
   dotnet run
   ```

8. **Access the application**
   - Navigate to `https://localhost:5001` in your browser
   - Click "Login" to authenticate via Auth0

## 🔐 User Roles & Permissions

The application uses Auth0 for authentication and implements role-based access control:

### Player Role
- View league standings and statistics
- Access personal performance data
- View team information and schedules

### Captain Role
- All Player permissions, plus:
- Manage team roster (add/remove players)
- Access detailed team analytics
- View team-specific administrative data

### Administrator Role
- All Captain and Player permissions, plus:
- Enter and modify game scores
- Create and manage teams
- Manage league settings and configuration
- Access user management features
- Generate comprehensive reports

### Authentication Features
- **Secure Login**: Industry-standard OAuth 2.0 authentication via Auth0
- **Multiple Login Options**: Email/password, Google, Facebook, and other social providers
- **Account Management**: Users can update profiles and manage account settings
- **Password Security**: Auth0 handles password policies and security best practices
- **Session Management**: Automatic session handling and secure logout

## 📋 Usage

### For Players
- View current league standings
- Check upcoming game schedules
- Review personal and team statistics
- Access league rules and information

### For Team Captains
- Manage team rosters
- View team performance metrics
- Access team-specific statistics
- Review game schedules

### For Administrators
- Enter weekly game scores
- Manage teams and players
- Configure league settings
- Generate reports and statistics

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

### Development Guidelines
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Contact

**Project Maintainer**: SithLord2K  
**Project Link**: [https://github.com/SithLord2K/WellandPoolLeagueMud](https://github.com/SithLord2K/WellandPoolLeagueMud)  
**Live Application**: [https://wpl.codersden.com](https://wpl.codersden.com)

## 🙏 Acknowledgments

- [Auth0](https://auth0.com/) for providing robust authentication and authorization services
- [MudBlazor](https://mudblazor.com/) for the excellent UI component library
- [Microsoft Blazor](https://blazor.net/) for the powerful web framework
- The local pool league community for inspiration and testing

---

**Built for the Welland Pool League community**