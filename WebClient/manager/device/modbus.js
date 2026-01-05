// JavaScript source code
import { JsonModal } from '/components/jsonModal/jsonModal.js';
import '/components/jsonModal/jsonModal.css';
//import { ModbusRegisterConfigurator } from '/components/modbus/modbus.js';
import { ModbusConfigEditor } from '/components/modbus/modbusConfigEditor.js'
import '/components/modbus/modbusConfigEditor.css';
const jsonModal = new JsonModal();
export function loadModbusConfigPage() {

    const modbusConfigJson = {
        "protocol_version": "1.0",
        "data_types": {
            "bit": { "size": 1, "signed": false },
            "int8": { "size": 1, "signed": true },
            "uint8": { "size": 1, "signed": false },
            "int16": { "size": 2, "signed": true },
            "uint16": { "size": 2, "signed": false },
            "float32": { "size": 4, "format": "ieee754" },
            "bitmask16": { "size": 2, "format": "bitmask" }
        },
        "registers": [
            { "address": 0, "type": "int16", "scale": 10.0, "byte_order": "ABCD", "name": "ambient_temp" }
        ],
        "byte_order_presets": { "default": "ABCD" }
    }

    //const configurator = new ModbusRegisterConfigurator('modbus-config', protocol);
    const editor = new ModbusConfigEditor(
        document.getElementById( 'modbus-editor'),
        modbusConfigJson);

    const topBox = document.getElementById('topBox');

    topBox.addEventListener('click', (e) => {
        if (e.target.id === 'btnAddRegister') {
            editor.addRegister();
        }
        if (e.target.id === 'btnPrintConfig') {
            const json = configurator.exportConfig();
            jsonModal.show(json);
        }
    });
}

