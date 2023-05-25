Конфигурация сервиса
Добавляем к config файл блок внутрь configuration:
===
<!--
Для доступа по IP: 192.168.101.12:7000
Для доступа по COM порту: COM6
-->
<applicationSettings>
    <WindowsFormsApp1.Properties.Settings>
        <setting name="converterAddress" serializeAs="String">
            <value>192.168.101.12:7000</value>
        </setting>
    </WindowsFormsApp1.Properties.Settings>
</applicationSettings>