# 🐦‍⬛ CleanCrow - Otimizador de Sistema

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![Windows](https://img.shields.io/badge/Windows-10%2B-blue)](https://www.microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)

Ferramenta profissional para limpeza e otimização do Windows, desenvolvida em C# com WPF.

## 📋 Funcionalidades

### 🧹 Limpeza de Sistema
- Arquivos temporários
- Logs do sistema
- Cache do Windows Update
- Cache DNS
- Cache de navegadores (Edge, Chrome, Firefox, Opera, Brave, Vivaldi)
- Lixeira
- Programas desnecessários
- Bloatware do Windows

### 🔄 Atualização de Programas
- Atualização via Winget (Windows Package Manager)
- Interface moderna e intuitiva
- Logs detalhados de todas as operações
- Monitoramento de espaço em disco

## 🚀 Como Usar

### Download
Baixe a última versão em [Releases](https://github.com/seu-usuario/CleanCrow/releases)

### Executar
1. Execute o arquivo `CleanCrow.exe`
2. O Windows solicitará permissão de administrador
3. Clique em "Sim" para continuar
4. Use os botões para limpar ou atualizar o sistema

### Compilar do Código Fonte
```bash
# Clone o repositório
git clone https://github.com/seu-usuario/CleanCrow.git

# Entre na pasta
cd CleanCrow

# Restaure os pacotes
dotnet restore

# Compile
dotnet build -c Release

# Execute
dotnet run